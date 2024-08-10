using FileStorageApi.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;
using Dapper;
using Npgsql;

namespace FileStorageApi.Data.Repositories;

internal class UserRepository : RepositoryBase, IUserRepository
{
	public UserRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction)
		: base(connection, transaction)
	{	}
	
	public async Task CreateUserAsync(User user, CancellationToken ct)
	{
		const string query = """
			INSERT INTO "user" (id, email, is_confirmed, username, password_hash, created_at, modified_at, folder_id)
			VALUES (@id, @email, @isConfirmed, @username, @passwordHash, @createdAt, @modifiedAt, @folderId);
			""";
		
		await Connection.ExecuteAsync(new CommandDefinition(query, user, Transaction, cancellationToken: ct));
	}
	
	public async Task<bool> EmailExists(string emailAddress, CancellationToken ct)
	{
		const string query = "SELECT EXISTS (SELECT 1 FROM \"user\" WHERE email = @emailAddress)";
		
		return await Connection.ExecuteScalarAsync<bool>(
			new CommandDefinition(query, new { emailAddress }, Transaction, cancellationToken: ct));
	}
	
	public async Task<bool> UsernameExists(string username, CancellationToken ct)
	{
		const string query = "SELECT EXISTS (SELECT 1 FROM \"user\" WHERE username = @username)";
		
		return await Connection.ExecuteScalarAsync<bool>(
			new CommandDefinition(query, new { username }, Transaction, cancellationToken: ct));
	}

	public async Task<User?> GetByEmail(string emailAddress, CancellationToken ct)
	{
		const string query = "SELECT * FROM \"user\" where email = @emailAddress;";
		
		var user = await Connection.QuerySingleOrDefaultAsync<User>(
			new CommandDefinition(query, new { emailAddress }, Transaction, cancellationToken: ct));
		
		return user;
	}
	
	public Task<Guid> GetFolderId(Guid userId, CancellationToken ct)
	{
		throw new NotImplementedException();
	}
	
}