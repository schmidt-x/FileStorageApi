using System.Threading.Tasks;
using App.Domain.Entities;
using System.Threading;
using Dapper;
using Npgsql;

namespace App.Data.Repositories;

internal class UserRepository : RepositoryBase, IUserRepository
{
	public UserRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction)
		: base(connection, transaction)
	{	}
	
	public async Task CreateUserAsync(User user, CancellationToken ct)
	{
		const string query = """
			INSERT INTO "user" (id, email, is_confirmed, username, password, created_at, modified_at, folder_id)
			VALUES (@id, @email, @isConfirmed, @username, @passwordHash, @createdAt, @modifiedAt, @folderId);
			""";
		
		await Connection.ExecuteAsync(new CommandDefinition(query, user, Transaction, cancellationToken: ct));
	}
}