using ValidationException = App.Common.Exceptions.ValidationException;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using App.Infrastructure;
using FluentValidation;
using System.Threading;
using App.Services;
using App.Common;
using App.Data;
using Serilog;

namespace App.Features.Auth.Commands;

public record LoginUserCommand(string Login, string Password);

public class LoginUser : RequestHandlerBase
{
	private readonly IValidator<LoginUserCommand> _validator;
	private readonly IRepositoryService _repoService;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ILogger _logger;
	
	public LoginUser(
		IValidator<LoginUserCommand> validator,
		IRepositoryService repoService,
		IPasswordHasher passwordHasher,
		ILogger logger)
	{
		_validator = validator;
		_repoService = repoService;
		_passwordHasher = passwordHasher;
		_logger = logger;
	}
	
	public async Task<Result<IEnumerable<Claim>>> Handle(LoginUserCommand request, CancellationToken ct)
	{
		var validationResult = _validator.Validate(request);
		
		if (!validationResult.IsValid)
		{
			return new ValidationException("auth", ["Invalid credentials."]);
		}
		
		var user = await _repoService.Users.GetByEmail(request.Login, ct);
		
		if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
		{
			return new ValidationException("auth", ["Invalid credentials."]);
		}
		
		_logger.Information("User {username} (Id: {id}) has logged in.", user.Username, user.Id);
		
		Claim[] claims = [ 
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
		];
		
		return claims;
	}
}