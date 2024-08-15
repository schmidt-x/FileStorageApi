using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using FileStorageApi.Features.Auth.Services;
using FileStorageApi.Domain.Entities;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Features.Auth.Contracts;
using FileStorageApi.Options;
using FileStorageApi.Common;
using FileStorageApi.Data;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Serilog;
using System;
using FluentValidation;

namespace FileStorageApi.Features.Auth.Commands.CreateUser;

public record CreateUserCommand(string Email, string Username, string Password);

public class CreateUserHandler : RequestHandlerBase
{
	private readonly IValidator<CreateUserCommand> _validator;
	private readonly IRepositoryContext _db;
	private readonly StorageOptions _storageOpts;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ILogger _logger;
	private readonly IAuthSchemeProvider _authSchemeProvider;
	
	public CreateUserHandler(
		IValidator<CreateUserCommand> validator,
		IRepositoryContext db,
		IOptions<StorageOptions> storageOpts,
		IPasswordHasher passwordHasher,
		ILogger logger,
		IAuthSchemeProvider authSchemeProvider)
	{
		_db = db;
		_validator = validator;
		_storageOpts = storageOpts.Value;
		_passwordHasher = passwordHasher;
		_logger = logger;
		_authSchemeProvider = authSchemeProvider;
	}
	
	public async Task<Result<ClaimsPrincipal>> Handle(CreateUserCommand request, CancellationToken ct)
	{
		var validationResult = _validator.Validate(request);
		
		if (!validationResult.IsValid)
		{
			return new ValidationException(validationResult.Errors);
		}
		
		if (await _db.Users.EmailExists(request.Email, ct))
		{
			return new ValidationException("email", ["Email address is already taken."]);
		}
		
		if (await _db.Users.UsernameExists(request.Username, ct))
		{
			return new ValidationException("username", ["Username is already taken."]);
		}
		
		var timeNow = DateTimeOffset.UtcNow;
		
		var user = new User
		{
			Id = Guid.NewGuid(),
			Email = request.Email,
			IsConfirmed = false,
			Username = request.Username,
			PasswordHash = _passwordHasher.Hash(request.Password),
			CreatedAt = timeNow,
			ModifiedAt = timeNow,
			FolderId = Guid.NewGuid()
		};
		
		var folder = new Folder
		{
			Id = user.FolderId,
			Name = "/",
			Path = "",
			Size = 0,
			IsTrashed = false,
			CreatedAt = timeNow,
			ModifiedAt = timeNow,
			ParentId = Guid.Empty,
			UserId = user.Id
		};
		
		var userStorageFolder = Path.Combine(_storageOpts.StorageFolder, user.FolderId.ToString());
		
		Directory.CreateDirectory(userStorageFolder);
		
		await _db.BeginTransactionAsync(ct);
		
		try
		{
			await _db.Users.CreateUserAsync(user, ct);
			await _db.Folders.CreateFolder(folder, ct);
			await _db.SaveChangesAsync(ct);
		}
		catch
		{
			// TODO: log
			await _db.UndoChangesAsync();
			Directory.Delete(userStorageFolder);
			
			throw;
		}
		
		_logger.Information("User {username} (Id: {id}) has registered.", user.Username, user.Id);
		
		Claim[] claims = [
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) 
		];
		
		var identity = new ClaimsIdentity(claims, _authSchemeProvider.Scheme);
		
		return new ClaimsPrincipal(identity);
	}
}







