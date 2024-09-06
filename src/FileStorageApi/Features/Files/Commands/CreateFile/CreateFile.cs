using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using File = FileStorageApi.Domain.Entities.File;
using FileStorageApi.Features.Files.Exceptions;
using FileStorageApi.Features.Files.Services;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Features.Files.Helpers;
using FileStorageApi.Common.Services;
using FileStorageApi.Common.Helpers;
using Microsoft.Extensions.Options;
using FileStorageApi.Common.Options;
using System.Threading.Tasks;
using FileStorageApi.Common;
using FileStorageApi.Data;
using FluentValidation;
using System.Threading;
using System.IO;
using Serilog;
using System;

namespace FileStorageApi.Features.Files.Commands.CreateFile;

public record CreateFileCommand(Stream File, string FileName, string MimeType, string? Folder);

public record CreatedFile(string Name, string Folder, string FullName, long Size, DateTimeOffset CreatedAt);

public class CreateFileCommandHandler : RequestHandlerBase
{
	private readonly IValidator<CreateFileCommand> _validator;
	private readonly IFileSignature _fileSignature;
	private readonly IRepositoryContext _db;
	private readonly IUser _user;
	private readonly StorageOptions _storageOpts;
	private readonly ILogger _logger;
	
	public CreateFileCommandHandler(
		IValidator<CreateFileCommand> validator,
		IFileSignature fileSignature,
		IRepositoryContext db,
		IUser user,
		IOptions<StorageOptions> storageOpts,
		ILogger logger)
	{
		_validator = validator;
		_fileSignature = fileSignature;
		_db = db;
		_user = user;
		_storageOpts = storageOpts.Value;
		_logger = logger;
	}
	
	public async Task<Result<CreatedFile>> Handle(CreateFileCommand command, CancellationToken ct)
	{
		var validationResult = _validator.Validate(command);
		if (!validationResult.IsValid)
		{
			return new ValidationException(validationResult.Errors);
		}
		
		var rawFileName = Path.Join(command.Folder, command.FileName);
		
		var fileInfoResult = FilePathInfo.New(rawFileName, _storageOpts.PathSegmentMaxLength);
		if (fileInfoResult.IsError(out var ex))
		{
			return ex;
		}
		
		var fileInfo = fileInfoResult.Value;
		var folderInfo = fileInfo.Folder;
		
		var fileTypeResult = _fileSignature.Validate(command.File, fileInfo.Extension, command.MimeType);
		if (fileTypeResult.IsError(out ex))
		{
			return ex;
		}
		
		var fileType = fileTypeResult.Value;
		var rootFolderId = _user.FolderId();
		
		if (await NotEnoughSpace(rootFolderId, command.File.Length, ct))
		{
			return new StorageOutOfSpaceException();
		}
		
		var userId = _user.Id();
		
		var folderId = folderInfo.IsRootFolder
			? rootFolderId
			: await _db.Folders.GetIdIfFolderExistsAsync(folderInfo.Path, folderInfo.Name, userId, ct);
		
		if (!folderId.HasValue)
		{
			return new FolderNotFoundException(folderInfo.FullName);
		}
		
		if (await _db.Files.ExistsAsync(fileInfo.Name, fileInfo.Extension, folderId.Value, userId, ct))
		{
			return new DuplicateFileNameException(fileInfo.NameWithExtension);
		}
		
		var timeNow = DateTimeOffset.UtcNow;
		
		var file = new File
		{
			Id = Guid.NewGuid(),
			Name = fileInfo.Name,
			Extension = fileInfo.Extension,
			Size = command.File.Length,
			Type = fileType,
			IsTrashed = false,
			CreatedAt = timeNow,
			ModifiedAt = timeNow,
			FolderId = folderId.Value,
			UserId = userId
		};
		
		await CreateFileAsync(file, command.File, rootFolderId, userId, ct);
		
		// TODO: encode fileName before displaying?
		_logger.Information(
			"User {userId} created new file: {fileName} (Id: {fileId}).", userId, fileInfo.FullName, file.Id);
		
		return new CreatedFile(
			fileInfo.NameWithExtension,
			folderInfo.FullName,
			fileInfo.FullName,
			file.Size,
			file.CreatedAt);
	}
	
	private async Task<bool> NotEnoughSpace(Guid folderId, long fileSize, CancellationToken ct)
	{
		var folderSize = await _db.Folders.GetSizeAsync(folderId, ct);
		
		return folderSize + fileSize > _storageOpts.StorageSizeLimitPerUser;
	}
	
	private async Task CreateFileAsync(File fileEntity, Stream file, Guid rootFolderId, Guid userId, CancellationToken ct)
	{
		var fileId = fileEntity.Id.ToString();
		
		var filePath = FilePathGenerator.Generate(fileId);
		var fileDirectoryPath = Path.Combine(_storageOpts.StorageFolder, rootFolderId.ToString(), filePath);
		
		if (!Directory.Exists(fileDirectoryPath))
		{
			Directory.CreateDirectory(fileDirectoryPath);
		}
		
		var fileFullPath = Path.Combine(fileDirectoryPath, fileId);
		
		await using (var fileStream = System.IO.File.Create(fileFullPath))
		{
			await file.CopyToAsync(fileStream, ct);
		}
		
		bool duringTransaction = false;
		
		try
		{
			await _db.BeginTransactionAsync(ct);
			
			duringTransaction = true;
			await _db.Files.CreateAsync(fileEntity, ct);
			await _db.Folders.IncreaseSizeAsync(fileEntity.FolderId, fileEntity.Size, ct);
			duringTransaction = false;
			
			await _db.SaveChangesAsync(ct);
		}
		catch (Exception exception)
		{
			if (duringTransaction) await _db.UndoChangesAsync();
			System.IO.File.Delete(fileFullPath);
			
			_logger.Error("Failed at creating file for user {userId}: {errorMessage}.", userId, exception.Message);
			throw;
		}
	}
}