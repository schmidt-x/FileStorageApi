using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileStorageApi.Domain.Entities;
using FileStorageApi.Domain.Enums;
using FileStorageApi.Domain.Models;

namespace FileStorageApi.Data.Repositories;

public interface IFolderRepository
{
	Task CreateFolderAsync(Folder folder, CancellationToken ct);
	Task<long> GetSizeAsync(Guid folderId, CancellationToken ct);
	Task<Guid?> GetIdIfFolderExistsAsync(string path, string name, Guid userId, CancellationToken ct);
	Task<Guid> GetIdAsync(string path, string name, Guid userId, CancellationToken ct);
	Task IncreaseSizeAsync(Guid folderId, long size, Guid? upToFolderId, CancellationToken ct);
	Task DecreaseSizeAsync(Guid folderId, long size, Guid? upToFolderId, CancellationToken ct);
	Task<bool> ExistsAsync(string path, string name, Guid userId, CancellationToken ct);
	Task<Guid?> GetPathIdIfExistsAsync(string path, Guid userId, CancellationToken ct);
	Task<List<Item>> GetItemsAsync(Guid folderId, int? limit, int? offset, ItemOrder? orderBy, bool? desc, CancellationToken ct);
	Task<int> CountItemsAsync(Guid folderId, CancellationToken ct);
	Task<Folder?> GetFolderIfExists(string path, string name, Guid userId, CancellationToken ct);
}