using System;
using System.Threading;
using System.Threading.Tasks;
using FileStorageApi.Domain.Entities;

namespace FileStorageApi.Data.Repositories;

public interface IFolderRepository
{
	Task CreateFolderAsync(Folder folder, CancellationToken ct);
	Task<long> GetSizeAsync(Guid folderId, CancellationToken ct);
	Task<Guid?> GetIdIfFolderExistsAsync(string path, string name, Guid userId, CancellationToken ct);
	Task<Guid> GetIdAsync(string path, string name, Guid userId, CancellationToken ct);
	Task IncreaseSizeAsync(Guid folderId, long size, CancellationToken ct);
	Task<bool> ExistsAsync(string path, string name, Guid userId, CancellationToken ct);
	Task<Guid?> GetPathIdIfExistsAsync(string path, Guid userId, CancellationToken ct);
}