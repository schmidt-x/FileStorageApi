using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using FileStorageApi.Common;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Common.Options;
using FileStorageApi.Common.Services;
using FileStorageApi.Data;
using FileStorageApi.Domain.Entities;
using FileStorageApi.Features.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.Options;
using Serilog;

namespace FileStorageApi.Features.Folders.Commands.CreateFolder;

public record CreateFolderCommand(string FolderName);

public record CreatedFolder(string Name, string Path, string FullName, DateTimeOffset CreatedAt);

public class CreateFolderCommandHandler : RequestHandlerBase
{
	private readonly IValidator<CreateFolderCommand> _validator;
	private readonly StorageOptions _storageOpts;
	private readonly IRepositoryContext _db;
	private readonly IUser _user;
	private readonly TimeProvider _time;
	private readonly ILogger _logger;
	
	public CreateFolderCommandHandler(
		IValidator<CreateFolderCommand> validator,
		IOptions<StorageOptions> options,
		IRepositoryContext db,
		IUser user,
		TimeProvider time,
		ILogger logger)
	{
		_validator = validator;
		_storageOpts = options.Value;
		_db = db;
		_user = user;
		_time = time;
		_logger = logger;
	}
	
	public async Task<Result<CreatedFolder>> Handle(CreateFolderCommand command, CancellationToken ct)
	{
		var validationResult = _validator.Validate(command);
		if (!validationResult.IsValid)
		{
			return new ValidationException(validationResult.Errors);
		}
		
		var folderInfoResult = FolderPathInfo.New(command.FolderName, _storageOpts.PathSegmentMaxLength);
		if (folderInfoResult.IsError(out var ex))
		{
			return ex;
		}
		
		var folderInfo = folderInfoResult.Value;
		
		if (folderInfo.IsRootFolder)
		{
			return new DuplicateFolderNameException(folderInfo.Name);
		}
		
		var userId = _user.Id();
		var timeNow = _time.GetUtcNow();
		var pathId = await _db.Folders.GetPathIdIfExistsAsync(folderInfo.Path, userId, ct);
			
		if (pathId.HasValue)
		{
			if (await _db.Folders.ExistsAsync(folderInfo.Path, folderInfo.Name, userId, ct))
			{
				return new DuplicateFolderNameException(folderInfo.FullName);
			}
			
			await CreateFolder(folderInfo, pathId.Value, timeNow, userId, ct);
		}
		else
		{
			await RecursivelyCreateFolders(folderInfo, timeNow, userId, ct);
		}
		
		_logger.Information("User {userId} created folder {folderFullName}.", userId, folderInfo.FullName);
		
		return new CreatedFolder(folderInfo.Name, folderInfo.Path, folderInfo.FullName, timeNow);
	}
	
	private async Task CreateFolder(
		FolderPathInfo folderInfo,
		Guid pathId,
		DateTimeOffset timeNow,
		Guid userId,
		CancellationToken ct)
	{
		_ = folderInfo.TryGetParent(out var parentInfo);
		
		var parentId = parentInfo!.IsRootFolder
			? _user.FolderId()
			: await _db.Folders.GetIdAsync(parentInfo.Path, parentInfo.Name, userId, ct);
		
		var folder = new Folder
		{
			Id = Guid.NewGuid(),
			Name = folderInfo.Name,
			PathId = pathId,
			CreatedAt = timeNow,
			ModifiedAt = timeNow,
			ParentId = parentId,
			UserId = userId
		};
		
		try
		{
			await _db.Folders.CreateFolderAsync(folder, ct);
		}
		catch (Exception ex)
		{
			_logger.Error("Failed at creating folder for user {userId}: {errorMessage}.", userId, ex.Message);
			throw;
		}
	}
	
	private async Task RecursivelyCreateFolders(
		FolderPathInfo folderInfo,
		DateTimeOffset timeNow,
		Guid userId,
		CancellationToken ct)
	{
		var foldersToCreate = new Stack<Folder>();
		
		var folderPath = new FolderPath(Guid.NewGuid(), folderInfo.Path, userId);
		
		foldersToCreate.Push(new Folder
		{
			Id = Guid.NewGuid(),
			Name = folderInfo.Name,
			PathId = folderPath.Id,
			CreatedAt = timeNow,
			ModifiedAt = timeNow,
			UserId = userId,
			FolderPath = folderPath
		});
		
		var parentId = Guid.Empty;
		
		// Iterate through parent folders until we reach a folder that exists (at worst case it's 'root' folder).
		// Additionally, on each iteration, add folders into the Stack to create them later.
		// When we finally reach the folder that exists, take its Id as 'parentId' and start creating folders
		// that we've previously added. The first folder to create will have 'parentId' that we have just found.
		// All further folders will have their 'parentId' the Id of a folder that was created in the previous iteration.
		
		while (folderInfo.TryGetParent(out var parentInfo))
		{
			if (parentInfo.IsRootFolder)
			{
				parentId = _user.FolderId();
				break;
			}
			
			var pathId = await _db.Folders.GetPathIdIfExistsAsync(parentInfo.Path, userId, ct);
			if (!pathId.HasValue)
			{
				// Neither Path nor Folder exists.
				// Hence, add both.
			
				folderPath = new FolderPath(Guid.NewGuid(), parentInfo.Path, userId);
				
				foldersToCreate.Push(new Folder
				{
					Id = Guid.NewGuid(),
					Name = parentInfo.Name,
					PathId = folderPath.Id,
					CreatedAt = timeNow,
					ModifiedAt = timeNow,
					UserId = userId,
					FolderPath = folderPath
				});
				
				folderInfo = parentInfo;
				continue;
			}
			
			var folderId = await _db.Folders.GetIdIfFolderExistsAsync(parentInfo.Path, parentInfo.Name, userId, ct);
			if (folderId.HasValue) // Both Path and Folder exist.
			{
				parentId = folderId.Value;
				break;
			}
			
			// Path exists but Folder doesn't.
			
			foldersToCreate.Push(new Folder
			{
				Id = Guid.NewGuid(),
				Name = parentInfo.Name,
				PathId = pathId.Value,
				CreatedAt = timeNow,
				ModifiedAt = timeNow,
				UserId = userId
			});
			
			_ = parentInfo.TryGetParent(out var lastParentInfo);
			
			parentId = lastParentInfo!.IsRootFolder
				? _user.FolderId()
				: await _db.Folders.GetIdAsync(lastParentInfo.Path, lastParentInfo.Name, userId, ct);
			
			break;
		}
		
		bool duringTransaction = false;
		
		try
		{
			await _db.BeginTransactionAsync(ct);
			
			duringTransaction = true;
			while (foldersToCreate.Count != 0)
			{
				var folder = foldersToCreate.Pop();
				folder.ParentId = parentId;
				
				await _db.Folders.CreateFolderAsync(folder, ct);
				parentId = folder.Id;
			}
			duringTransaction = false;
			
			await _db.SaveChangesAsync(ct);
		}
		catch (Exception ex)
		{
			if (duringTransaction) await _db.UndoChangesAsync();
			_logger.Information(
				"Failed at recursively creating folders for user {userId}: {errorMessage}.", userId, ex.Message);
			
			throw;
		}
	}
}
