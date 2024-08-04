using System.Text.RegularExpressions;
using App.Features.Auth.Commands;
using FluentValidation;

namespace App.Features.Auth.Validation;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
	public CreateUserCommandValidator()
	{
		RuleFor(u => u.Email)
			.NotEmpty().OverridePropertyName("email").WithMessage("Email address must not be empty.")
			.EmailAddress().WithMessage("Invalid email address.");
		
		RuleFor(u => u.Username)
			.NotEmpty().OverridePropertyName("username").WithMessage("Username must not be empty.")
			.MinimumLength(3).WithMessage("Username must not be shorter than 3 characters.")
			.MaximumLength(32).WithMessage("Username must not be longer than 32 characters.")
			.Matches("^[a-zA-Z0-9_.]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
				.WithMessage("Username can only contain letters, numbers, underscores, or periods.");
		
		RuleFor(u => u.Password)
			.NotEmpty().OverridePropertyName("password").WithMessage("Password must not be empty.")
			.MinimumLength(8).WithMessage("Password must not be shorter than 8 characters.")
			.Custom((password, context) =>
			{
				foreach (var failure in Helpers.ValidatePassword(password))
				{
					context.AddFailure(failure);
				}
			}).When(x => !string.IsNullOrEmpty(x.Password), ApplyConditionTo.CurrentValidator);
	}
}