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
			.GreaterThan(0)
			.WithMessage("File is empty.")
			.OverridePropertyName("EmptyFile");
		
		RuleFor(x => x.File.Length)
			.LessThanOrEqualTo(fileSizeLimit)
			.WithMessage($"File size exceeds the limit of {fileSizeLimit} bytes.")
			.OverridePropertyName("FileTooLarge");
		
		RuleFor(x => x.FileName)
			.NotEmpty()
			.WithMessage("FileName is empty.")
			.OverridePropertyName("EmptyFileName");
		
		RuleFor(x => x.FileName)
			.MaximumLength(fileNameMaxLen)
			.WithMessage($"FileName's length exceeds the limit of {fileNameMaxLen} characters.")
			.OverridePropertyName("FileNameTooLong");
		
		RuleFor(x => x.FileName)
			.Must(fileName => !fileName.ContainsControlChars())
			.WithMessage("FileName contains Control characters.")
			.OverridePropertyName("InvalidFileName");
		
		RuleFor(x => x.FileName)
			.Must((command, fileName) => IsValidFullPathLength(fileName, command.Folder, fullPathMaxLen))
			.WithMessage($"Path's total length exceeds the limit of {fullPathMaxLen} characters.")
			.OverridePropertyName("PathTooLong");
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