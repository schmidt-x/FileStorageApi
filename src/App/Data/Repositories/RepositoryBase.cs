using Npgsql;

namespace App.Data.Repositories;

internal abstract class RepositoryBase
{
	private readonly NpgsqlConnection _connection;
	
	protected NpgsqlTransaction? Transaction { get; }
	
	protected NpgsqlConnection Connection =>
		Transaction is not null ? Transaction.Connection! : _connection;
	
	protected RepositoryBase(NpgsqlConnection connection, NpgsqlTransaction? transaction)
	{
		_connection = connection;
		Transaction = transaction;
	}
}