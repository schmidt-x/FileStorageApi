using System.Threading.Tasks;
using System.Threading;
using System;

namespace FileStorageApi.Data.Repositories;

public interface IFileRepository
{
	Task<bool> FileExists(string fileName, string folderName, Guid userId, CancellationToken ct);
	Task AddFile(string tempName, string fileName, string folderName, Guid userId, CancellationToken ct);
}