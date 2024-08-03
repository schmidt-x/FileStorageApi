using System.Threading.Tasks;
using App.Data.Repositories;
using System.Threading;

namespace App.Data;

public interface IRepositoryService
{
	IUserRepository Users { get; }
	
	Task BeginTransactionAsync(CancellationToken ct);
	Task SaveChangesAsync(CancellationToken ct);
	Task UndoChangesAsync();
}
