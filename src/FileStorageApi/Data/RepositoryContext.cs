using FileStorageApi.Data.Repositories;
using System.Threading.Tasks;
using System.Transactions;
using System.Threading;
using System;
using Npgsql;

namespace FileStorageApi.Data;

public class RepositoryContext : IRepositoryContext, IDisposable
{
	private readonly NpgsqlConnection _connection;
	private NpgsqlTransaction? _transaction;
	
	private IUserRepository? _userRepository;
	private IFileRepository? _fileRepository;
	
	public RepositoryContext(NpgsqlDataSource dataSource)
	{
		_connection = dataSource.CreateConnection();
	}
	
	public IUserRepository Users 
		=> _userRepository ??= new UserRepository(_connection, _transaction);
	
	public IFileRepository Files 
		=> _fileRepository ??= new FileRepository(_connection, _transaction);
	
	public async Task BeginTransactionAsync(CancellationToken ct)
	{
		await _connection.OpenAsync(ct);
		_transaction = await _connection.BeginTransactionAsync(ct);
		ResetRepositories();
	}
	
	public async Task SaveChangesAsync(CancellationToken ct)
	{
		if (_transaction is null)
			throw new TransactionException("Transaction hasn't been started.");
		
		try
		{
			await _transaction.CommitAsync(ct);
		}
		finally
		{
			await _transaction.DisposeAsync();
			_transaction = null;
			await _connection.CloseAsync();
			ResetRepositories();
		}
	}
	
	public async Task UndoChangesAsync()
	{
		if (_transaction is null)
			throw new TransactionException("Transaction hasn't been started.");
		
		try
		{
			await _transaction.RollbackAsync();
		}
		finally
		{
			await _transaction.DisposeAsync();
			_transaction = null;
			await _connection.CloseAsync();
			ResetRepositories();
		}
	}
	
	public void Dispose()
	{
		_transaction?.Dispose();
		_connection.Dispose();
	}
	
	private void ResetRepositories()
	{
		_userRepository = null;
		_fileRepository = null;
	}
}