using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Exceptions.FileExceptions;
using File = FileStorageApi.Domain.Entities.File;
using FileStorageApi.Features.Files.Services;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Common.Services;
using FileStorageApi.Common.Helpers;
using Microsoft.Extensions.Options;
using FileStorageApi.Common.Options;
using FileStorageApi.Domain.Enums;
using System.Threading.Tasks;
using FileStorageApi.Data;
using FluentValidation;
using System.Threading;
using System.IO;
using Serilog;
using System;

namespace FileStorageApi.Features.Files.Commands.CreateFile;

public record CreateFileCommand(Stream File, string FileName, string MimeType, string? Folder);

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
	
	public async Task<Exception?> Handle(CreateFileCommand command, CancellationToken ct)
	{
		var validationResult = _validator.Validate(command);
		if (!validationResult.IsValid)
		{
			return new ValidationException(validationResult.Errors);
		}
		
		var fileExt = Path.GetExtension(command.FileName).ToLower();
		
		var fileTypeResult = _fileSignature.Validate(command.File, fileExt, command.MimeType);
		if (fileTypeResult.IsError(out var ex))
		{
			return ex;
		}
		
		FileType fileType = fileTypeResult.Value;
		
		var rootFolderId = _user.FolderId();
		
		var size = await _db.Folders.GetSizeAsync(rootFolderId, ct);
		if (size + command.File.Length >= _storageOpts.StorageSizeLimitPerUser)
		{
			// TODO: return more specific exception
			return new Exception("Not enough space.");
		}
		
		var folderInfoResult = FolderPathInfo.New(command.Folder, _storageOpts.PathSegmentMaxLength);
		if (folderInfoResult.IsError(out ex))
		{
			return ex;
		}
		
		FolderPathInfo folderInfo = folderInfoResult.Value;
		var userId = _user.Id();
		
		var folderId = await _db.Folders.GetIdIfFolderExistsAsync(folderInfo.Path, folderInfo.Name, userId, ct);
		if (!folderId.HasValue)
		{
			return new FolderNotFoundException($"Folder '{folderInfo.FullName}' does not exist.");
		}
		
		var fileName = Path.GetFileNameWithoutExtension(command.FileName);
		
		if (await _db.Files.ExistsAsync(fileName, fileExt, folderId.Value, userId, ct))
		{
			return new DuplicateFileNameException($"File with the name '{command.FileName}' already exists.");
		}
		
		var timeNow = DateTimeOffset.UtcNow;
		
		var file = new File
		{
			Id = Guid.NewGuid(),
			Name = fileName,
			Extension = fileExt,
			Size = command.File.Length,
			Type = fileType,
			IsTrashed = false,
			CreatedAt = timeNow,
			ModifiedAt = timeNow,
			FolderId = folderId.Value,
			UserId = userId
		};
		
		var fileId = file.Id.ToString();
		var filePath = $@"{fileId[..2]}\{fileId[2..4]}";
		
		var fileDirectoryPath = Path.Combine(_storageOpts.StorageFolder, rootFolderId.ToString(), filePath);
		
		if (!Directory.Exists(fileDirectoryPath))
		{
			Directory.CreateDirectory(fileDirectoryPath);
		}
		
		var fileFullPath = Path.Combine(fileDirectoryPath, fileId);
		
		await using (var fileStream = System.IO.File.Create(fileFullPath))
		{
			await command.File.CopyToAsync(fileStream, ct);
		}
		
		bool duringTransaction = false;
		
		try
		{
			await _db.BeginTransactionAsync(ct);
			
			duringTransaction = true;
			await _db.Files.CreateAsync(file, ct);
			await _db.Folders.IncreaseSizeAsync(folderId.Value, file.Size, ct);
			duringTransaction = false;
			
			await _db.SaveChangesAsync(ct);
		}
		catch (Exception exception)
		{
			if (duringTransaction) await _db.UndoChangesAsync();
			System.IO.File.Delete(fileFullPath);
			
			_logger.Error("Failed at creating file for user {userId}: {errorMessage}", userId, exception.Message);
			throw;
		}
		
		// TODO: encode fileName before displaying?
		_logger.Information(
			"User {userId} created new file: {fileName} (Id: {fileId}).", userId, command.FileName, file.Id);
		
		return null;
	}
}