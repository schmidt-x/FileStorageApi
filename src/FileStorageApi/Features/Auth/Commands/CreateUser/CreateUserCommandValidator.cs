using FileStorageApi.Features.Auth.Helpers;
using System.Text.RegularExpressions;
using FluentValidation;

namespace FileStorageApi.Features.Auth.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
	public CreateUserCommandValidator()
	{
		RuleFor(u => u.Email)
			.NotEmpty().WithMessage("Email address must not be empty.")
			.EmailAddress().WithMessage("Invalid email address.");
		
		RuleFor(u => u.Username)
			.NotEmpty().WithMessage("Username must not be empty.")
			.MinimumLength(3).WithMessage("Username must not be shorter than 3 characters.")
			.MaximumLength(32).WithMessage("Username must not be longer than 32 characters.")
			.Matches("^[a-zA-Z0-9_.]+$", RegexOptions.Compiled)
				.WithMessage("Username can only contain letters, numbers, underscores, or periods.");
		
		RuleFor(u => u.Password)
			.NotEmpty().WithMessage("Password must not be empty.")
			.MinimumLength(8).WithMessage("Password must not be shorter than 8 characters.")
			.Custom((password, context) =>
			{
				foreach (var failure in PasswordValidator.Validate(password))
				{
					context.AddFailure(failure);
				}
			}).When(x => !string.IsNullOrEmpty(x.Password), ApplyConditionTo.CurrentValidator);
	}
}