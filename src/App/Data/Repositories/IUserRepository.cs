using System.Threading.Tasks;
using App.Domain.Entities;
using System.Threading;

namespace App.Data.Repositories;

public interface IUserRepository
{
	Task CreateUserAsync(User user, CancellationToken ct);
	Task<bool> EmailExists(string emailAddress, CancellationToken ct);
	Task<bool> UsernameExists(string username, CancellationToken ct);
}