using App.Features.Auth.Commands;
using FluentValidation;

namespace App.Features.Auth.Validation;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
	public LoginUserCommandValidator()
	{
		RuleFor(x => x.Login).NotEmpty().EmailAddress();
		
		RuleFor(x => x.Password)
			.NotEmpty()
			.MinimumLength(8)
			.Custom((password, context) =>
			{
				if (Helpers.ValidatePassword(password).Length != 0)
				{
					context.AddFailure("Fail");
				}
			}).When(x => !string.IsNullOrEmpty(x.Password), ApplyConditionTo.CurrentValidator);
	}
}