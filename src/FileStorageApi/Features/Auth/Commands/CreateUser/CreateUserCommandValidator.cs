using static FileStorageApi.Domain.Constants.ErrorCodes;
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
			.NotEmpty()
			.WithName(EMPTY_VALUE)
			.WithMessage("Email address is required.");
		
		RuleFor(x => x.Email)
			.EmailAddress()
			.WithName(INVALID_VALUE)
			.WithMessage("Invalid email address.");
		
		RuleFor(x => x.Username)
			.NotEmpty()
			.WithName(EMPTY_VALUE)
			.WithMessage("Username is required.");
		
		RuleFor(x => x.Username)
			.MinimumLength(authOpts.UsernameMinLength)
			.WithName(LENGTH_BELOW_MINIMUM)
			.WithMessage($"Minimum length is {authOpts.UsernameMinLength} characters.");
		
		RuleFor(x => x.Username)
			.MaximumLength(authOpts.UsernameMaxLength)
			.WithName(LENGTH_EXCEEDS_LIMIT)
			.WithMessage($"Maximum length is {authOpts.UsernameMaxLength} characters.");
		
		RuleFor(x => x.Username)
			.Matches(@"^[a-zA-Z\d_.]+$", RegexOptions.Compiled)
			.WithName(INVALID_VALUE)
			.WithMessage("Username can only contain letters, numbers, underscores, or periods.");
		
		RuleFor(x => x.Password)
			.NotEmpty()
			.WithName(EMPTY_VALUE)
			.WithMessage("Password is required.");
		
		RuleFor(x => x.Password)
			.MinimumLength(authOpts.PasswordMinLength)
			.WithName(LENGTH_BELOW_MINIMUM)
			.WithMessage($"Minimum length for password is {authOpts.PasswordMinLength} characters.");
		
		RuleFor(x => x.Password)
			.Matches(@"\d", RegexOptions.Compiled)
			.WithName(VALUE_MISSING_DIGIT)
			.WithMessage("Password must have at least one digit ('0'-'9').");
		
		RuleFor(x => x.Password)
			.Matches("[A-Z]", RegexOptions.Compiled)
			.WithName(VALUE_MISSING_UPPERCASE)
			.WithMessage("Password must have at least one uppercase ('A'-'Z').");
		
		RuleFor(x => x.Password)
			.Matches("[a-z]", RegexOptions.Compiled)
			.WithName(VALUE_MISSING_LOWERCASE)
			.WithMessage("Password must have at least one lowercase ('a'-'z').");
		
		RuleFor(x => x.Password)
			.Matches(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled)
			.WithName(VALUE_MISSING_SYMBOL)
			.WithMessage("Password must have at least one non-alphanumeric character.");
		
		RuleFor(x => x.Password)
			.Matches(@"^\S*$", RegexOptions.Compiled)
			.WithName(INVALID_VALUE)
			.WithMessage("Password must not have any whitespaces.");
	}
}