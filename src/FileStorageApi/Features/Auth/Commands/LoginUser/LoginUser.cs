using System.Security.Authentication;
using System.Threading.Tasks;
using System.Security.Claims;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Features.Auth.Services;
using FluentValidation;
using System.Threading;
using FileStorageApi.Data;
using FileStorageApi.Common;
using FileStorageApi.Domain.Constants;
using Serilog;

namespace FileStorageApi.Features.Auth.Commands.LoginUser;

public record LoginUserCommand(string Login, string Password);

public class LoginUserCommandHandler : RequestHandlerBase
{
	private readonly IValidator<LoginUserCommand> _validator;
	private readonly IRepositoryContext _db;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ILogger _logger;
	private readonly IAuthSchemeProvider _authSchemeProvider;
	
	public LoginUserCommandHandler(
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
			return new AuthenticationException();
		}
		
		var user = await _db.Users.GetByEmail(request.Login, ct);
		
		if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
		{
			return new AuthenticationException();
		}
		
		_logger.Information("User {username} (Id: {id}) has logged in.", user.Username, user.Id);
		
		Claim[] claims = [ 
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(AuthClaims.FolderId, user.FolderId.ToString())
		];
		
		var identity = new ClaimsIdentity(claims, _authSchemeProvider.Scheme);
		
		return new ClaimsPrincipal(identity);
	}
}