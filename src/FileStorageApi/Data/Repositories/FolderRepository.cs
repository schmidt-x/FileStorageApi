using FileStorageApi.Domain.Entities;
using FileStorageApi.Data.Extensions;
using FileStorageApi.Domain.Models;
using FileStorageApi.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Dapper;
using Npgsql;

namespace FileStorageApi.Data.Repositories;

public class FolderRepository : RepositoryBase, IFolderRepository
{
	public FolderRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction)
		: base(connection, transaction) { }
	
	public async Task CreateFolderAsync(Folder folder, CancellationToken ct)
	{
		const string insertPath = """
			INSERT INTO paths (id, path, user_id)
			VALUES (@PathId, @Path, @UserId);
			""";
		
		const string insertFolder = """
			INSERT INTO folders (id, name, path_id, size, is_trashed, created_at, modified_at, parent_id, user_id)
			VALUES (@Id, @Name, @PathId, @Size, @IsTrashed, @CreatedAt, @ModifiedAt, @ParentId, @UserId);

			UPDATE folders
			SET modified_at = @CreatedAt
			WHERE id = @ParentId;
			""";
		
		var parameters = new DynamicParameters(folder);
		string query;
		
		if (folder.FolderPath is not null)
		{
			query = insertPath + "\n\n" + insertFolder;
			parameters.Add(nameof(folder.FolderPath.Path), folder.FolderPath.Path);
		}
		else
		{
			query = insertFolder;
		}
		
		await Connection.ExecuteAsync(new CommandDefinition(query, parameters, Transaction, cancellationToken: ct));
	}
	
	public async Task<long> GetSizeAsync(Guid folderId, CancellationToken ct)
	{
		const string query = "SELECT size FROM folders WHERE id = @FolderId";
		
		var command = new CommandDefinition(query, new { folderId }, Transaction, cancellationToken: ct);
		
		return await Connection.ExecuteScalarAsync<long>(command);
	}
	
	public async Task<Guid?> GetIdIfFolderExistsAsync(string path, string name, Guid userId, CancellationToken ct)
	{
		const string query = """
			WITH
				_path AS (SELECT id FROM paths WHERE (path, user_id) = (@Path, @UserId))
				
				SELECT id FROM folders
				WHERE (name, path_id, user_id, is_trashed) = (@Name, (SELECT id FROM _path), @UserId, false);
			""";
		
		var command = new CommandDefinition(query, new { path, name, userId }, Transaction, cancellationToken: ct);
		
		return await Connection.QuerySingleOrDefaultAsync<Guid?>(command);
	}
	
	public async Task<Guid> GetIdAsync(string path, string name, Guid userId, CancellationToken ct)
	{
		return await GetIdIfFolderExistsAsync(path, name, userId, ct) 
			?? throw new InvalidOperationException("Sequence contains no elements");
	}
	
	public async Task<bool> ExistsAsync(string path, string name, Guid userId, CancellationToken ct)
	{
		const string query = """
			WITH
				_path AS (SELECT id FROM paths WHERE (path, user_id) = (@Path, @UserId))

				SELECT EXISTS(
					SELECT 1 FROM folders
					WHERE (name, path_id, user_id, is_trashed) = (@Name, (SELECT id FROM _path), @UserId, false));
			""";
		
		var command = new CommandDefinition(query, new { path, name, userId }, Transaction, cancellationToken: ct);
		
		return await Connection.ExecuteScalarAsync<bool>(command);
	}
	
	public async Task<Guid?> GetPathIdIfExistsAsync(string path, Guid userId, CancellationToken ct)
	{
		const string query = "SELECT id FROM paths WHERE (path, user_id) = (@Path, @UserId);";
		
		return await Connection.QuerySingleOrDefaultAsync<Guid?>(
			new CommandDefinition(query, new { path, userId }, Transaction, cancellationToken: ct));
	}
	
	public Task DecreaseSizeAsync(Guid folderId, long size, Guid? upToFolderId, CancellationToken ct)
	{
		return IncreaseSizeAsync(folderId, -size, upToFolderId, ct);
	}
	
	public async Task IncreaseSizeAsync(Guid folderId, long size, Guid? upToFolderId, CancellationToken ct)
	{
		string whereClause = upToFolderId.HasValue
			? "id <> @UpToFolderId AND id IN (SELECT id FROM _folders);"
			: "id IN (SELECT id FROM _folders);";
		
		string query = $"""
			WITH RECURSIVE _folders AS (
				SELECT id, parent_id FROM folders WHERE id = @FolderId
				UNION ALL
				SELECT f.id, f.parent_id FROM folders f
				JOIN _folders _f ON _f.parent_id = f.id
			)
			
			UPDATE folders
			SET size = size + @Size
			WHERE {whereClause}
			""";
		
		var command = new CommandDefinition(
			query, new { folderId, size, upToFolderId }, Transaction, cancellationToken: ct);
		
		await Connection.ExecuteAsync(command);
	}
	
	public async Task<List<Item>> GetItemsAsync(
		Guid folderId, int? limit, int? offset, ItemOrder? orderBy, bool? desc, CancellationToken ct)
	{
		string query = $"""
			SELECT name, size, 'Folder'::itemtype AS type, created_at, modified_at
			FROM folders
			WHERE parent_id = @FolderId
			
			UNION ALL
			
			SELECT name || extension, size, 'File'::itemtype as type, created_at, modified_at
			FROM files
			WHERE folder_id = @FolderId
			
			ORDER BY {(orderBy ?? ItemOrder.Type).ToDbName()} {(desc ?? false ? "DESC" : "")}
			LIMIT {(limit.HasValue ? limit.Value.ToString() : "ALL")} OFFSET {offset ?? 0};
			""";
		
		var command = new CommandDefinition(query, new { folderId }, Transaction, cancellationToken: ct);
		
		// As long as the command is 'CommandFlags.Buffered' (it is, by default), the return type is List<T>
		return (List<Item>)await Connection.QueryAsync<Item>(command);
	}
	
	public async Task<int> CountItemsAsync(Guid folderId, CancellationToken ct)
	{
		const string query = """
			SELECT
				(SELECT count(*) FROM folders WHERE parent_id = @FolderId AND is_trashed = FALSE) + 
				(SELECT count(*) FROM files WHERE folder_id = @FolderId AND is_trashed = FALSE) 
			""";
		
		var command = new CommandDefinition(query, new { folderId }, Transaction, cancellationToken: ct);
		
		return await Connection.ExecuteScalarAsync<int>(command);
	}
	
	public async Task<Folder?> GetFolderIfExists(string path, string name, Guid userId, CancellationToken ct)
	{
		const string query = """
			WITH
				_path AS (SELECT id FROM paths WHERE (path, user_id) = (@Path, @UserId))
				
				SELECT * FROM folders
				WHERE (name, path_id, user_id, is_trashed) = (@Name, (select id from _path), @UserId, false);
			""";
		
		var command = new CommandDefinition(query, new { path, name, userId }, Transaction, cancellationToken: ct);
		
		return await Connection.QuerySingleOrDefaultAsync<Folder>(command);
	}
}