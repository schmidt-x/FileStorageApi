using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using FileStorageApi.Features.Auth.Services;
using FileStorageApi.Domain.Entities;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Common.Options;
using FileStorageApi.Common;
using FileStorageApi.Data;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Serilog;
using System;
using FileStorageApi.Domain.Constants;
using FluentValidation;

namespace FileStorageApi.Features.Auth.Commands.CreateUser;

public record CreateUserCommand(string Email, string Username, string Password);

public class CreateUserCommandHandler : RequestHandlerBase
{
	private readonly IValidator<CreateUserCommand> _validator;
	private readonly IRepositoryContext _db;
	private readonly StorageOptions _storageOpts;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ILogger _logger;
	private readonly IAuthSchemeProvider _authSchemeProvider;
	
	public CreateUserCommandHandler(
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
			return new ValidationException(nameof(request.Email), ["Email address is already taken."]);
		}
		
		if (await _db.Users.UsernameExists(request.Username, ct))
		{
			return new ValidationException(nameof(request.Username), ["Username is already taken."]);
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
		
		var folderPath = new FolderPath(Guid.NewGuid(), "", user.Id);
		
		var folder = new Folder
		{
			Id = user.FolderId,
			Name = "/",
			PathId = folderPath.Id,
			Size = 0,
			IsTrashed = false,
			CreatedAt = timeNow,
			ModifiedAt = timeNow,
			ParentId = Guid.Empty,
			UserId = user.Id,
			FolderPath = folderPath
		};
		
		var folderIdStr = user.FolderId.ToString();
		var userStorageFolder = Path.Combine(_storageOpts.StorageFolder, folderIdStr);
		
		Directory.CreateDirectory(userStorageFolder);
		
		bool duringCreation = false;
		
		try
		{
			await _db.BeginTransactionAsync(ct);
			
			duringCreation = true;
			await _db.Users.CreateUserAsync(user, ct);
			await _db.Folders.CreateFolderAsync(folder, ct);
			duringCreation = false;
			
			await _db.SaveChangesAsync(ct);
		}
		catch (Exception ex)
		{
			if (duringCreation) await _db.UndoChangesAsync();
			Directory.Delete(userStorageFolder);
			
			_logger.Error("Failed to create a user: {errorMessage}", ex.Message);
			throw;
		}
		
		_logger.Information("User {username} (Id: {id}) has registered.", user.Username, user.Id);
		
		Claim[] claims = [
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(AuthClaims.FolderId, folderIdStr)
		];
		
		var identity = new ClaimsIdentity(claims, _authSchemeProvider.Scheme);
		
		return new ClaimsPrincipal(identity);
	}
}
