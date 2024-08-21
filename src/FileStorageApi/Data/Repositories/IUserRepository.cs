using FileStorageApi.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;

namespace FileStorageApi.Data.Repositories;

public interface IUserRepository
{
	Task CreateUserAsync(User user, CancellationToken ct);
	Task<bool> EmailExists(string emailAddress, CancellationToken ct);
	Task<bool> UsernameExists(string username, CancellationToken ct);
	Task<User?> GetByEmail(string emailAddress, CancellationToken ct);
}