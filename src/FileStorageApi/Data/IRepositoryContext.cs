using System.Threading.Tasks;
using FileStorageApi.Data.Repositories;
using System.Threading;

namespace FileStorageApi.Data;

public interface IRepositoryContext
{
	IUserRepository Users { get; }
	IFileRepository Files { get; }
	
	Task BeginTransactionAsync(CancellationToken ct);
	Task SaveChangesAsync(CancellationToken ct);
	Task UndoChangesAsync();
}
