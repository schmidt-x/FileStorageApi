using FileStorageApi.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;
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
}