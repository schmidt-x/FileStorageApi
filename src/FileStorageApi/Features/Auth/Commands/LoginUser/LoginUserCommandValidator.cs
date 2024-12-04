using FileStorageApi.Common.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace FileStorageApi.Features.Auth.Commands.LoginUser;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
	public LoginUserCommandValidator(IOptions<AuthOptions> authOpts)
	{
		RuleFor(x => x.Login).NotEmpty().EmailAddress();
		
		RuleFor(x => x.Password)
			.NotEmpty()
			.MinimumLength(authOpts.Value.PasswordMinLength)
			.Matches("[a-z]")
			.Matches("[A-Z]")
			.Matches(@"\d")
			.Matches(@"[^a-zA-Z0-9\s]")
			.Matches(@"^\S*$");
	}
}