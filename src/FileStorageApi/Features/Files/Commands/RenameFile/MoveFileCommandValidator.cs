using FileStorageApi.Common.Options;
using Microsoft.Extensions.Options;
using FluentValidation;

namespace FileStorageApi.Features.Files.Commands.RenameFile;

public class MoveFileCommandValidator : AbstractValidator<MoveFileCommand>
{
	public MoveFileCommandValidator(IOptions<StorageOptions> options)
	{
		var fullPathMaxLen = options.Value.FullPathMaxLength;
		
		RuleFor(x => x.FileName)
			.NotEmpty()
			.WithMessage("FileName is empty.")
			.OverridePropertyName("EmptyFileName");
		
		RuleFor(x => x.FileName)
			.MaximumLength(fullPathMaxLen)
			.WithMessage($"Path's length exceeds the limit of {fullPathMaxLen} characters.")
			.OverridePropertyName("PathTooLong");
		
		RuleFor(x => x.DestFileName)
			.NotEmpty()
			.WithMessage("New FileName is empty.")
			.OverridePropertyName("EmptyNewFileName");
		
		RuleFor(x => x.DestFileName)
			.MaximumLength(fullPathMaxLen)
			.WithMessage($"Path's length exceeds the limit of {fullPathMaxLen} characters.")
			.OverridePropertyName("NewPathTooLong");
	}
}