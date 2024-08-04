using ValidationException = App.Common.Exceptions.ValidationException;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using App.Domain.Entities;
using App.Infrastructure;
using System.Threading;
using FluentValidation;
using App.Services;
using App.Options;
using App.Common;
using System.IO;
using App.Data;
using Serilog;
using System;

namespace App.Features.Auth.Commands;

public record CreateUserCommand(string Email, string Username, string Password);

public class CreateUserHandler : RequestHandlerBase
{
	private readonly IValidator<CreateUserCommand> _validator;
	private readonly TimeProvider _timeProvider;
	private readonly IRepositoryService _repoService;
	private readonly StorageOptions _storageOpts;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ILogger _logger;
	
	public CreateUserHandler(
		IValidator<CreateUserCommand> validator,
		TimeProvider timeProvider,
		IRepositoryService repoService,
		IOptions<StorageOptions> storageOpts,
		IPasswordHasher passwordHasher,
		ILogger logger)
	{
		_repoService = repoService;
		_validator = validator;
		_timeProvider = timeProvider;
		_storageOpts = storageOpts.Value;
		_passwordHasher = passwordHasher;
		_logger = logger;
	}
	
	public async Task<Result<IEnumerable<Claim>>> Handle(CreateUserCommand request, CancellationToken ct)
	{
		var validationResult = _validator.Validate(request);
		
		if (!validationResult.IsValid)
		{
			return new ValidationException(validationResult.Errors);
		}
		
		if (await _repoService.Users.EmailExists(request.Email, ct))
		{
			return new ValidationException("email", ["Email address is already taken."]);
		}
		
		if (await _repoService.Users.UsernameExists(request.Username, ct))
		{
			return new ValidationException("username", ["Username is already taken."]);
		}
		
		var timeNow = _timeProvider.GetUtcNow();
		
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
		
		var userFolderIdStr = user.FolderId.ToString();
		
		var userStorageFolder = Path.Combine(_storageOpts.RootFolder, _storageOpts.StorageFolder, userFolderIdStr);
		var userTrashFolder = Path.Combine(_storageOpts.RootFolder, _storageOpts.TrashFolder, userFolderIdStr);
		
		Directory.CreateDirectory(userStorageFolder);
		Directory.CreateDirectory(userTrashFolder);
		
		try
		{
			await _repoService.Users.CreateUserAsync(user, ct);
		}
		catch
		{
			Directory.Delete(userStorageFolder);
			Directory.Delete(userTrashFolder);
			throw;
		}
		
		_logger.Information("User {username} (Id: {id}) has registered.", user.Username, user.Id);
		
		Claim[] claims = [
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) 
		];
		
		return claims;
	}
}







