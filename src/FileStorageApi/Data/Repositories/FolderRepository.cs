using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FileStorageApi.Domain.Entities;
using Npgsql;

namespace FileStorageApi.Data.Repositories;

public class FolderRepository : RepositoryBase, IFolderRepository
{
	public FolderRepository(NpgsqlConnection connection, NpgsqlTransaction? transaction)
		: base(connection, transaction) { }
	
	public async Task CreateFolder(Folder folder, CancellationToken ct)
	{
		const string query = """
			INSERT INTO folder (id, name, path, size, is_trashed, created_at, modified_at, parent_id, user_id)
			VALUES (@Id, @Name, @Path, @Size, @IsTrashed, @CreatedAt, @ModifiedAt, @ParentId, @UserId);
			""";
		
		await Connection.ExecuteAsync(new CommandDefinition(query, folder, Transaction, cancellationToken: ct));
	}
}