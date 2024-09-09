using FileStorageApi.Common.Options;
using Microsoft.Extensions.Options;
using FluentValidation;

namespace FileStorageApi.Features.Files.Queries.DownloadFile;

public class DownloadFileQueryValidator : AbstractValidator<DownloadFileQuery>
{
	public DownloadFileQueryValidator(IOptions<StorageOptions> options)
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
	}
}