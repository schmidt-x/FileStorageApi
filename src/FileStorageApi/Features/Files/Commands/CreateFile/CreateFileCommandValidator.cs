using FileStorageApi.Common.Extensions;
using Microsoft.Extensions.Options;
using FileStorageApi.Common.Options;
using FluentValidation;

namespace FileStorageApi.Features.Files.Commands.CreateFile;

public class CreateFileCommandValidator : AbstractValidator<CreateFileCommand>
{
	public CreateFileCommandValidator(IOptions<StorageOptions> storageOpts)
	{
		var opts = storageOpts.Value;
		
		var fileSizeLimit = opts.FileSizeLimit;
		var fullPathMaxLen = opts.FullPathMaxLength;
		var fileNameMaxLen = opts.FileNameMaxLength;
		
		RuleFor(x => x.File.Length)
			.GreaterThan(0).OverridePropertyName("File").WithMessage("File is empty.")
			.LessThanOrEqualTo(fileSizeLimit).WithMessage($"File size exceeds {fileSizeLimit} bytes limit.");
		
		RuleFor(x => x.FileName)
			.NotEmpty().WithMessage("File name is empty.")
			.MaximumLength(fileNameMaxLen).WithMessage($"FileName's length exceeds the limit of {fileNameMaxLen} characters.")
			.Must(fileName => !fileName.ContainsControlChars()).WithMessage("File name contains Control characters.")
			.Must((command, _) => command.FileName.Length + (command.Folder?.Length ?? 1) <= fullPathMaxLen)
				.WithMessage($"Path's total length exceeds the limit of {fullPathMaxLen} characters.");
	}
}