using FileStorageApi.Features.Auth.Helpers;

using FluentValidation;

namespace FileStorageApi.Features.Auth.Commands.LoginUser;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
	public LoginUserCommandValidator()
	{
		RuleFor(x => x.Login).NotEmpty().EmailAddress();
		
		RuleFor(x => x.Password)
			.NotEmpty()
			.MinimumLength(8)
			.Must(password => PasswordValidator.Validate(password).Length == 0);
	}
}