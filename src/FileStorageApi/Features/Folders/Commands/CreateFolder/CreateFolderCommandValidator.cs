using FileStorageApi.Common.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace FileStorageApi.Features.Folders.Commands.CreateFolder;

public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
	public CreateFolderCommandValidator(IOptions<StorageOptions> options)
	{
		var fullPathMaxLen = options.Value.FullPathMaxLength;
		
		RuleFor(x => x.FolderName)
			.NotEmpty()
			.WithMessage("FolderName is empty.")
			.OverridePropertyName("EmptyFolderName");
		
		RuleFor(x => x.FolderName)
			.Must(folderName => IsValidFolderFullNameLength(folderName, fullPathMaxLen))
			.WithMessage($"FolderName's length exceeds the FullPathMaxLength limit of {fullPathMaxLen}" +
			             " characters, leaving no characters for filename.")
			.OverridePropertyName("PathTooLong");
	}
	
	private static bool IsValidFolderFullNameLength(string folderName, int maxLength)
	{
		var prefix = folderName.StartsWith('/') || folderName.StartsWith('\\') ? 0 : 1;
		var postfix = folderName.EndsWith('/') || folderName.EndsWith('\\') ? 0 : 1;
		
		var totalLength = prefix + folderName.Length + postfix;
		
		return totalLength <= maxLength-1; // leaving 1 character for filename
	}
}