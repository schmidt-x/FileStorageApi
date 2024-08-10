using ValidationException = FileStorageApi.Common.Exceptions.ValidationException;
using System.Threading.Tasks;
using System.Security.Claims;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Features.Auth.Contracts;
using FileStorageApi.Features.Auth.Services;
using FluentValidation;
using System.Threading;
using FileStorageApi.Data;
using FileStorageApi.Common;
using Serilog;

namespace FileStorageApi.Features.Auth.Commands;

public record LoginUserCommand(string Login, string Password);

public class LoginUser : RequestHandlerBase
{
	private readonly IValidator<LoginUserCommand> _validator;
	private readonly IRepositoryContext _db;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ILogger _logger;
	private readonly IAuthSchemeProvider _authSchemeProvider;
	
	
	public LoginUser(
		IValidator<LoginUserCommand> validator,
		IRepositoryContext db,
		IPasswordHasher passwordHasher,
		ILogger logger,
		IAuthSchemeProvider authSchemeProvider)
	{
		_validator = validator;
		_db = db;
		_passwordHasher = passwordHasher;
		_logger = logger;
		_authSchemeProvider = authSchemeProvider;
	}
	
	public async Task<Result<ClaimsPrincipal>> Handle(LoginUserCommand request, CancellationToken ct)
	{
		var validationResult = _validator.Validate(request);
		
		if (!validationResult.IsValid)
		{
			return new ValidationException("auth", ["Invalid credentials."]);
		}
		
		var user = await _db.Users.GetByEmail(request.Login, ct);
		
		if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
		{
			return new ValidationException("auth", ["Invalid credentials."]);
		}
		
		_logger.Information("User {username} (Id: {id}) has logged in.", user.Username, user.Id);
		
		Claim[] claims = [ 
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
		];
		
		var identity = new ClaimsIdentity(claims, _authSchemeProvider.Scheme);
		
		return new ClaimsPrincipal(identity);
	}
}