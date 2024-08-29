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
			.Must(fileName => !fileName.ContainsControlChars()).WithMessage("FileName contains Control characters.")
			.Must((command, _) => IsValidFullPathLength(command.FileName, command.Folder, fullPathMaxLen))
				.WithMessage($"Path's total length exceeds the limit of {fullPathMaxLen} characters.");
	}
	
	private static bool IsValidFullPathLength(string fileName, string? folderName, int fullPathMaxLength)
	{
		int prefix, postfix;
		int folderNameLength;
		
		if (string.IsNullOrEmpty(folderName))
		{
			folderNameLength = 1;
			prefix = postfix = 0;
		}
		else
		{
			prefix = folderName.StartsWith('/') || folderName.StartsWith('\\') ? 0 : 1;
			postfix = folderName.EndsWith('/') || folderName.EndsWith('\\') ? 0 : 1;
			folderNameLength = folderName.Length;
		}
		
		var totalLength = prefix + folderNameLength + postfix + fileName.Length;
		
		return totalLength <= fullPathMaxLength;
	}
}