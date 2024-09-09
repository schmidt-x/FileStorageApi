using FileNotFoundException = FileStorageApi.Features.Files.Exceptions.FileNotFoundException;
using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using FileStorageApi.Features.Infrastructure;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using FileStorageApi.Common;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Common.Options;
using FileStorageApi.Common.Services;
using FileStorageApi.Data;
using FileStorageApi.Domain.Enums;
using FileStorageApi.Features.Files.Helpers;
using FileStorageApi.Features.Files.Services;
using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using Serilog;

namespace FileStorageApi.Features.Files.Queries.DownloadFile;

public record DownloadFileQuery(string FileName);
public record File(FileStream Stream, string Name, string MimeType);

public class DownloadFileQueryHandler : RequestHandlerBase
{
	private readonly IValidator<DownloadFileQuery> _validator;
	private readonly StorageOptions _storageOpts;
	private readonly IRepositoryContext _db;
	private readonly IUser _user;
	private readonly ILogger _logger;
	private readonly IFileSignature _fileSignature;
	
	public DownloadFileQueryHandler(
		IValidator<DownloadFileQuery> validator,
		IOptions<StorageOptions> storageOpts,
		IUser user,
		IRepositoryContext db,
		ILogger logger,
		IFileSignature fileSignature)
	{
		_storageOpts = storageOpts.Value;
		_validator = validator;
		_user = user;
		_db = db;
		_logger = logger;
		_fileSignature = fileSignature;
	}
	
	public async Task<Result<File>> Handle(DownloadFileQuery query, CancellationToken ct)
	{
		var validationResult = _validator.Validate(query);
		if (!validationResult.IsValid)
		{
			return new ValidationException(validationResult.Errors);
		}
		
		var fileInfoResult = FilePathInfo.New(
			query.FileName,
			_storageOpts.PathSegmentMaxLength,
			_storageOpts.FileNameMaxLength);
		
		if (fileInfoResult.IsError(out var ex))
		{
			return ex;
		}
		
		FilePathInfo fileInfo = fileInfoResult.Value;
		FolderPathInfo folderInfo = fileInfo.Folder;
		
		var userId = _user.Id();
		
		var folderId = await _db.Folders.GetIdIfFolderExistsAsync(folderInfo.Path, folderInfo.Name, userId, ct);
		if (!folderId.HasValue)
		{
			return new FolderNotFoundException(folderInfo.FullName);
		}
		
		var file = await _db.Files.GetFileIfExists(fileInfo.Name, fileInfo.Extension, folderId.Value, userId, ct);
		if (file is null)
		{
			return new FileNotFoundException(fileInfo.FullName);
		}
		
		var fileFullPath = GetFileFullPath(file.Id);
		
		if (!System.IO.File.Exists(fileFullPath))
		{
			_logger.Error(
				"File {fileName} with physical path {filePhysicalName} is present in Database, " +
				"but not in File System (User {userId}).",
				fileInfo.FullName, fileFullPath, _user.Id());
			
			throw new System.IO.FileNotFoundException(
				"Physical file is not found while being present in Database.", fileFullPath);
		}
		
		var mimeType = file.Type == FileType.Unknown
			? "application/octet-stream"
			: _fileSignature.GetMimeType(file.Extension)!;
		
		var fileStream = System.IO.File.OpenRead(fileFullPath);
		
		return new File(fileStream, fileInfo.NameWithExtension, mimeType);
	}
	
	private string GetFileFullPath(Guid fileId)
	{
		var fileName = fileId.ToString();
		var filePath = FilePathGenerator.Generate(fileName);
		
		return Path.Join(_storageOpts.StorageFolder, _user.FolderId().ToString(), filePath, fileName);
	}
}