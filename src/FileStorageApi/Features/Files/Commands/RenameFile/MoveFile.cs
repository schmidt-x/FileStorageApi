using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using System;
using System.Threading;
using System.Threading.Tasks;
using FileStorageApi.Common;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Common.Options;
using FileStorageApi.Common.Services;
using FileStorageApi.Data;
using FileStorageApi.Data.Repositories;
using FileStorageApi.Features.Files.Exceptions;
using FileStorageApi.Features.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace FileStorageApi.Features.Files.Commands.RenameFile;

public record MoveFileCommand(string FileName, string DestFileName);
public record UpdatedFile(string Name, string Path, long Size, DateTimeOffset CreatedAt, DateTimeOffset ModifiedAt);

public class MoveFileCommandHandler : RequestHandlerBase
{
	private readonly IValidator<MoveFileCommand> _validator;
	private readonly StorageOptions _storageOpts;
	private readonly IRepositoryContext _db;
	private readonly IUser _user;
	
	public MoveFileCommandHandler(
		IValidator<MoveFileCommand> validator,
		IOptions<StorageOptions> storageOpts,
		IRepositoryContext db,
		IUser user)
	{
		_validator = validator;
		_storageOpts = storageOpts.Value;
		_db = db;
		_user = user;
	}
	
	public async Task<Result<FileItem>> Handle(MoveFileCommand command, CancellationToken ct)
	{
		var validationResult = _validator.Validate(command);
		if (!validationResult.IsValid)
		{
			return new ValidationException(validationResult.Errors);
		}
		
		var fileInfoResult = FilePathInfo.New(
			command.FileName,
			_storageOpts.PathSegmentMaxLength,
			_storageOpts.FileNameMaxLength);
		
		if (fileInfoResult.IsError(out var ex)) return ex;
		
		FilePathInfo fileInfo = fileInfoResult.Value;
		FolderPathInfo folderInfo = fileInfo.Folder;
		
		var destFileInfoResult = FilePathInfo.New(
			command.DestFileName,
			_storageOpts.PathSegmentMaxLength,
			_storageOpts.FileNameMaxLength);
		
		if (destFileInfoResult.IsError(out ex)) return ex;
		
		var destFileInfo = destFileInfoResult.Value;
		var destFolderInfo = destFileInfo.Folder;
		
		var notMoving = folderInfo.Equals(destFolderInfo);
		var notRenaming = fileInfo.NameEquals(destFileInfo);
		
		if (notMoving && notRenaming)
		{
			return new Exception("Source and Destination names are the same.");
		}
		
		var userId = _user.Id();
		var rootFolderId = _user.FolderId();
		
		var folderId = folderInfo.IsRootFolder
			? rootFolderId
			: await _db.Folders.GetIdIfFolderExistsAsync(folderInfo.Path, folderInfo.Name, userId, ct);
			
		if (!folderId.HasValue)
		{
			return new FolderNotFoundException(folderInfo.FullName);
		}
		
		var fileId = await _db.Files.GetIdIfFileExistsAsync(fileInfo.Name, fileInfo.Extension, folderId.Value, userId, ct);
		if (!fileId.HasValue)
		{
			return new FileNotFoundException(fileInfo.FullName);
		}
		
		if (notMoving)
		{
			return await RenameFile(fileId.Value, destFileInfo.Name, ct);
		}
		
		var destFolderId = destFolderInfo.IsRootFolder
			? rootFolderId
			: await _db.Folders.GetIdIfFolderExistsAsync(destFolderInfo.Path, destFolderInfo.Name, userId, ct);
		
		if (!destFolderId.HasValue)
		{
			// return new DestinationFolderNotFoundException(destFolderInfo.FullName);
			return new Exception();
		}
		
		if (await _db.Files.ExistsAsync(destFileInfo.Name, destFileInfo.Extension, destFolderId.Value, userId, ct))
		{
			// return new DuplicateDestinationFileNameException(destFileInfo.NameWithExtension);
			return new Exception();
		}
		
		var lca = folderInfo.FindLCA(destFolderInfo); // Lowest Common Ancestor
		
		var lcaId = lca.IsRootFolder
			? rootFolderId
			: ReferenceEquals(lca, folderInfo)
				? folderId.Value
				: ReferenceEquals(lca, destFolderInfo)
					? destFolderId.Value
					: await _db.Folders.GetIdAsync(lca.Path, lca.Name, userId, ct);
		
		FileItem updatedFile;
		var duringTransaction = false;
		
		try
		{
			await _db.BeginTransactionAsync(ct);
			
			duringTransaction = true;
			updatedFile = notRenaming
				? await _db.Files.MoveAsync(fileId.Value, folderId.Value, destFolderId.Value, ct)
				: await _db.Files.MoveAndRenameAsync(fileId.Value, folderId.Value, destFolderId.Value, destFileInfo.Name, ct);
			
			if (folderId.Value != lcaId)
			{
				// decrease
			}
			
			if (destFolderId.Value != lcaId)
			{
				// increase
			}
			
			duringTransaction = false;
			
			await _db.SaveChangesAsync(ct);
		}
		catch (Exception exception)
		{
			Console.WriteLine(exception.Message);
			throw;
		}
		
		return updatedFile;
	}
	
	private async Task<FileItem> RenameFile(Guid fileId, string name, CancellationToken ct)
	{
		var fileItem = await _db.Files.RenameAsync(fileId, name, ct);
		
		// TODO: to log
		
		return fileItem;
	}
}
	