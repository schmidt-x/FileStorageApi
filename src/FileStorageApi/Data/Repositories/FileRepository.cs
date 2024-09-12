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
			VALUES (@Id, @Name, @Extension, @Size, @Type::filetype, @IsTrashed, @CreatedAt, @ModifiedAt, @FolderId, @UserId);
			
			UPDATE folders
			SET modified_at = @CreatedAt
			WHERE id = @FolderId;
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
	
	public async Task<Guid?> GetIdIfFileExistsAsync(
		string name, string extension, Guid folderId, Guid userId, CancellationToken ct)
	{
		const string query = """
			SELECT id FROM files
			WHERE (name, extension, folder_id, user_id, is_trashed) = (@Name, @Extension, @FolderId, @UserId, false);
			""";
		
		var command = new CommandDefinition(
			query,
			new { name, extension, folderId, userId },
			Transaction,
			cancellationToken: ct);
		
		return await Connection.QuerySingleOrDefaultAsync<Guid?>(command);
	}
	
	public async Task<FileItem> RenameAsync(Guid fileId, string newName, CancellationToken ct)
	{
		const string query = """
			WITH updated_file AS (
				UPDATE files
				SET name = @NewName, modified_at = NOW() AT TIME ZONE 'UTC'
				WHERE id = @FileId
				RETURNING folder_id, modified_at)

				UPDATE folders
				SET modified_at = (SELECT modified_at FROM updated_file)
				WHERE id = (SELECT folder_id FROM updated_file);

			SELECT f.name || f.extension AS name, p.path || fld.name AS folder, f.size, f.created_at, f.modified_at
			FROM files f
			JOIN folders fld ON fld.id = f.folder_id
			JOIN paths p ON p.id = fld.path_id
			WHERE f.id = @FileId;
			""";
		
		var command = new CommandDefinition(query, new { fileId, newName }, Transaction, cancellationToken: ct);
		
		return await Connection.QuerySingleAsync<FileItem>(command);
	}
	
	public async Task<FileItem> MoveAsync(Guid fileId, Guid folderId, Guid destFolderId, CancellationToken ct)
	{
		const string query = """
			WITH updated_file AS (
				UPDATE files
				SET folder_id = @DestFolderId, modified_at = NOW() AT TIME ZONE 'UTC'
				WHERE id = @FileId
				RETURNING modified_at)
				
				UPDATE folders
				SET modified_at = (SELECT modified_at FROM updated_file)
				WHERE id = @FolderId OR id = @DestFolderId;
			
			SELECT f.name || f.extension AS name, p.path || fld.name AS folder, f.size, f.created_at, f.modified_at
			FROM files f
			JOIN folders fld ON fld.id = f.folder_id
			JOIN paths p ON p.id = fld.path_id
			WHERE f.id = @FileId;
			""";
		
		var command = new CommandDefinition(
			query, new { fileId, folderId, destFolderId }, Transaction, cancellationToken: ct);
		
		return await Connection.QuerySingleAsync<FileItem>(command);
	}
	
	public async Task<FileItem> MoveAndRenameAsync(
		Guid fileId, Guid folderId, Guid destFolderId, string newName, CancellationToken ct)
	{
		const string query = """
			WITH updated_file AS (
				UPDATE files
				SET folder_id = @DestFolderId, name = @NewName, modified_at = NOW() AT TIME ZONE 'UTC'
				WHERE id = @FileId
				RETURNING modified_at)
				
				UPDATE folders
				SET modified_at = (SELECT modified_at FROM updated_file)
				WHERE id = @FolderId OR id = @DestFolderId;
			
			SELECT f.name || f.extension AS name, p.path || fld.name AS folder, f.size, f.created_at, f.modified_at
			FROM files f
			JOIN folders fld ON fld.id = f.folder_id
			JOIN paths p ON p.id = fld.path_id
			WHERE f.id = @FileId;
			""";
		
		var command = new CommandDefinition(
			query, new { fileId, folderId, destFolderId, newName }, Transaction, cancellationToken: ct);
		
		return await Connection.QuerySingleAsync<FileItem>(command);
	}
}

public record FileItem(string Name, string Folder, long Size, DateTimeOffset CreatedAt, DateTimeOffset ModifiedAt);