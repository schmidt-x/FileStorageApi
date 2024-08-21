using FileStorageApi.Features.Auth.Helpers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using FileStorageApi.Options;
using FluentValidation;

namespace FileStorageApi.Features.Auth.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
	public CreateUserCommandValidator(IOptions<AuthOptions> authOptions)
	{
		var authOpts = authOptions.Value;
		
		var usernameMinLen = authOpts.UsernameMinLength;
		var usernameMaxLen = authOpts.UsernameMaxLength;
		var passwordMinLen = authOpts.PasswordMinLength;
		
		RuleFor(u => u.Email)
			.NotEmpty().WithMessage("Empty email address.")
			.EmailAddress().WithMessage("Invalid email address.");
		
		RuleFor(u => u.Username)
			.NotEmpty().WithMessage("Username is empty.")
			.MinimumLength(usernameMinLen)
				.WithMessage($"Username is too short. Minimum length is {usernameMinLen} characters.")
			.MaximumLength(usernameMaxLen)
				.WithMessage($"Username is too long. Maximum length is {usernameMaxLen} characters.")
			.Matches("^[a-zA-Z0-9_.]+$", RegexOptions.Compiled)
				.WithMessage("Username can only contain letters, numbers, underscores, or periods.");
		
		RuleFor(u => u.Password)
			.NotEmpty().WithMessage("Password is empty.")
			.MinimumLength(passwordMinLen)
				.WithMessage($"Password is too short. Minimum length is {passwordMinLen} characters.")
			.Custom((password, context) =>
			{
				foreach (var failure in PasswordValidator.Validate(password))
				{
					context.AddFailure(failure);
				}
			}).When(x => !string.IsNullOrEmpty(x.Password), ApplyConditionTo.CurrentValidator);
	}
}