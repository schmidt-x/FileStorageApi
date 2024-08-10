using System.Threading.Tasks;
using System.Threading;
using System;
using Npgsql;

namespace FileStorageApi.Data.Repositories;

public class FileRepository : RepositoryBase, IFileRepository
{
	public FileRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction)
		: base(connection, transaction)	{	}
	
	public Task<bool> FileExists(string fileName, string folderName, Guid userId, CancellationToken ct)
	{
		throw new NotImplementedException();
	}
	
	public Task AddFile(string tempName, string fileName, string folderName, Guid userId, CancellationToken ct)
	{
		throw new NotImplementedException();
	}
}