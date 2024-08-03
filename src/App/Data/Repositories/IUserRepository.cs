using System.Threading.Tasks;
using App.Domain.Entities;
using System.Threading;

namespace App.Data.Repositories;

public interface IUserRepository
{
	Task CreateUserAsync(User user, CancellationToken ct);
}