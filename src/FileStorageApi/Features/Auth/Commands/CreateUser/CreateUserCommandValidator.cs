using System.Text.RegularExpressions;
using FileStorageApi.Common.Options;
using Microsoft.Extensions.Options;
using FluentValidation;

namespace FileStorageApi.Features.Auth.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
	public CreateUserCommandValidator(IOptions<AuthOptions> authOptions)
	{
		var authOpts = authOptions.Value;
		
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Empty email address.")
			.OverridePropertyName("EmptyEmail");
		
		RuleFor(x => x.Email)
			.EmailAddress().WithMessage("Invalid email address.")
			.OverridePropertyName("InvalidEmail");
		
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is empty.")
			.OverridePropertyName("EmptyUsername");
		
		RuleFor(x => x.Username)
			.MinimumLength(authOpts.UsernameMinLength)
			.WithMessage($"Minimum length is {authOpts.UsernameMinLength} characters.")
			.OverridePropertyName("UsernameTooShort");
		
		RuleFor(x => x.Username)
			.MaximumLength(authOpts.UsernameMaxLength)
			.WithMessage($"Maximum length is {authOpts.UsernameMaxLength} characters.")
			.OverridePropertyName("UsernameTooLong");
			
		RuleFor(x => x.Username)
			.Matches(@"^[a-zA-Z\d_.]+$", RegexOptions.Compiled)
			.WithMessage("Username can only contain letters, numbers, underscores, or periods.")
			.OverridePropertyName("InvalidUsername");
		
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is empty.")
			.OverridePropertyName("EmptyPassword");
		
		RuleFor(x => x.Password)
			.MinimumLength(authOpts.PasswordMinLength)
			.WithMessage($"Minimum length for password is {authOpts.PasswordMinLength} characters.")
			.OverridePropertyName("PasswordTooShort");
		
		RuleFor(x => x.Password)
			.Matches(@"\d", RegexOptions.Compiled)
			.WithMessage("Password must have at least one digit ('0'-'9').")
			.OverridePropertyName("NoDigitInPassword");
		
		RuleFor(x => x.Password)
			.Matches("[A-Z]", RegexOptions.Compiled)
			.WithMessage("Password must have at least one uppercase ('A'-'Z').")
			.OverridePropertyName("NoUpperInPassword");
		
		RuleFor(x => x.Password)
			.Matches("[a-z]", RegexOptions.Compiled)
			.WithMessage("Password must have at least one lowercase ('a'-'z').")
			.OverridePropertyName("NoLowerInPassword");
		
		RuleFor(x => x.Password)
			.Matches(@"[^\w\s]", RegexOptions.Compiled)
			.WithMessage("Password must have at least one non-alphanumeric character.")
			.OverridePropertyName("NoSymbolInPassword");
		
		RuleFor(x => x.Password)
			.Matches(@"^\S*$", RegexOptions.Compiled)
			.WithMessage("Password must not have any whitespaces.")
			.OverridePropertyName("WhitespaceInPassword");
	}
}