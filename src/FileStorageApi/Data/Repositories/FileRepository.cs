using FileStorageApi.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;
using Dapper;
using Npgsql;

namespace FileStorageApi.Data.Repositories;

public class FileRepository : RepositoryBase, IFileRepository
{
	public FileRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction)
		: base(connection, transaction)	{	}
	
	public async Task<bool> ExistsAsync(string name, string extension, Guid folderId, Guid userId, CancellationToken ct)
	{
		const string query = """
			SELECT EXISTS(
				SELECT 1 FROM files
				WHERE (name, extension, folder_id, user_id, is_trashed) = (@Name, @Extension, @FolderId, @UserId, false)
			);
			""";
		
		var command = new CommandDefinition(
			query,
			new { name, extension, folderId, userId },
			Transaction,
			cancellationToken: ct);
		
		return await Connection.ExecuteScalarAsync<bool>(command);
	}
	
	public async Task CreateAsync(File file, CancellationToken ct)
	{
		const string query = """
			INSERT INTO files (id, name, extension, size, type, is_trashed, created_at, modified_at, folder_id, user_id)
			VALUES (@Id, @Name, @Extension, @Size, @Type::filetype, @IsTrashed, @CreatedAt, @ModifiedAt, @FolderId, @UserId)
			""";
		
		// Since Dapper does not support custom type handlers for Enums (https://github.com/DapperLib/Dapper/issues/259),
		// we need to manually convert 'Type' enum to string (which will then be converted to 'filetype' enum type).
		var dParams = new DynamicParameters(file);
		dParams.Add(nameof(file.Type), file.Type.ToString());
		
		await Connection.ExecuteAsync(new CommandDefinition(query, dParams, Transaction, cancellationToken: ct));
	}
	
	public async Task<File?> GetFileIfExists(
		string name, string extension, Guid folderId, Guid userId, CancellationToken ct)
	{
		const string query = """
			SELECT * FROM files 
			WHERE (name, extension, folder_id, user_id, is_trashed) = (@Name, @Extension, @FolderId, @UserId, false);
			""";
		
		var command = new CommandDefinition(
			query,
			new { name, extension, folderId, userId },
			Transaction,
			cancellationToken: ct);
		
		return await Connection.QuerySingleOrDefaultAsync<File?>(command);
	}
}