using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Common.Services;
using FileStorageApi.Common.Models;
using System.Threading.Tasks;
using FileStorageApi.Common;
using System.Threading;
using System;
using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Helpers;
using FileStorageApi.Data;
using FileStorageApi.Domain.Enums;
using FileStorageApi.Domain.Models;

namespace FileStorageApi.Features.Folders.Queries.GetFolder;

public record GetFolderQuery(string? FolderName, int PageNumber, int PageSize, ItemOrder OrderBy, bool Desc);

public record FolderDto(
	string Name,
	long Size,
	DateTimeOffset CreatedAd,
	DateTimeOffset ModifiedAt,
	PaginatedList<Item> Content);

public class GetFolderQueryHandler : RequestHandlerBase
{
	private readonly IUser _user;
	private readonly IRepositoryContext _db;
	
	public GetFolderQueryHandler(
		IUser user,
		IRepositoryContext db)
	{
		_user = user;
		_db = db;
	}
	
	public async Task<Result<FolderDto>> Handle(GetFolderQuery query, CancellationToken ct)
	{
		var folderInfoResult = FolderPathInfo.New(query.FolderName);
		if (folderInfoResult.IsError(out var ex))
		{
			return ex;
		}
		
		var folderInfo = folderInfoResult.Value;
		var userId = _user.Id();
		
		var folder = await _db.Folders.GetFolderIfExists(folderInfo.Path, folderInfo.Name, userId, ct);
		if (folder is null)
		{
			return new FolderNotFoundException(folderInfo.FullName);
		}
		
		return new FolderDto(
			folderInfo.FullName,
			folder.Size,
			folder.CreatedAt,
			folder.ModifiedAt,
			await GetPaginatedList(folder.Id, query, ct));
	}
	
	private async Task<PaginatedList<Item>> GetPaginatedList(Guid folderId, GetFolderQuery query, CancellationToken ct)
	{
		var count = await _db.Folders.CountItemsAsync(folderId, ct);
		if (count == 0 || query.PageSize == 0)
		{
			return new PaginatedList<Item>([], count, 0, query.PageSize);
		}
		
		var offset = (query.PageNumber - 1) * query.PageSize;
		var items = await _db.Folders.GetItemsAsync(folderId, query.PageSize, offset, query.OrderBy, query.Desc, ct);
		
		return new PaginatedList<Item>(items, count, query.PageNumber, query.PageSize);
	}
}