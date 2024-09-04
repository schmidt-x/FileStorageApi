using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Features.Files.Exceptions;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Common.Services;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Common.Options;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using FileStorageApi.Common;
using FileStorageApi.Data;
using System.Threading;
using FluentValidation;
using System;

namespace FileStorageApi.Features.Files.Queries.GetFile;

public record GetFileQuery(string FileName);

public record FileDto(
	string FileName,
	string FolderName,
	long Size,
	DateTimeOffset CreatedAt,
	DateTimeOffset ModifiedAt);

public class GetFileQueryHandler : RequestHandlerBase
{
	private readonly IValidator<GetFileQuery> _validator;
	private readonly StorageOptions _storageOpts;
	private readonly IRepositoryContext _db;
	private readonly IUser _user;
	
	public GetFileQueryHandler(
		IValidator<GetFileQuery> validator,
		IOptions<StorageOptions> opts,
		IRepositoryContext db,
		IUser user)
	{
		_validator = validator;
		_storageOpts = opts.Value;
		_db = db;
		_user = user;
	}
	
	public async Task<Result<FileDto>> Handle(GetFileQuery query, CancellationToken ct)
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
		
		var fileInfo = fileInfoResult.Value;
		var folderInfo = fileInfo.Folder;
		
		var userId = _user.Id();
		
		var folderId = folderInfo.IsRootFolder
			? _user.FolderId()
			: await _db.Folders.GetIdIfFolderExistsAsync(folderInfo.Path, folderInfo.Name, userId, ct);
		
		if (!folderId.HasValue)
		{
			return new FolderNotFoundException(folderInfo.FullName);
		}
		
		var file = await _db.Files.GetFileIfExists(fileInfo.Name, fileInfo.Extension, folderId.Value, userId, ct);
		if (file is null)
		{
			return new FileNotFoundException(fileInfo.NameWithExtension);
		}
		
		return new FileDto(
			fileInfo.NameWithExtension,
			folderInfo.FullName,
			file.Size,
			file.CreatedAt,
			file.ModifiedAt);
	}
}