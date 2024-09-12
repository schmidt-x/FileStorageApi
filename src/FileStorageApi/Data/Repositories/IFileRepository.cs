using FileStorageApi.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace FileStorageApi.Data.Repositories;

public interface IFileRepository
{
	Task CreateAsync(File file, CancellationToken ct);
	Task<bool> ExistsAsync(string name, string extension, Guid folderId, Guid userId, CancellationToken ct);
	Task<File?> GetFileIfExists(string name, string extension, Guid folderId, Guid userId, CancellationToken ct);
	Task<Guid?> GetIdIfFileExistsAsync(string name, string extension, Guid folderId, Guid userId, CancellationToken ct);
	Task<FileItem> RenameAsync(Guid fileId, string name, CancellationToken ct);
	Task<FileItem> MoveAsync(Guid fileId, Guid folderId, Guid destFolderId, CancellationToken ct);
	Task<FileItem> MoveAndRenameAsync(Guid fileId, Guid folderId, Guid destFolderId, string newName, CancellationToken ct);
}