using Microsoft.Extensions.Options;
using System.Text;

namespace FileStorageApi.Options.Validation;

public class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
	public ValidateOptionsResult Validate(string? name, StorageOptions options)
	{
		var sb = new StringBuilder();
		
		if (string.IsNullOrWhiteSpace(options.StorageFolder))
		{
			sb.Append("\nStorageFolder is required.");
		}
		
		if (options.FileSizeLimit < 1)
		{
			sb.Append("\nFile size limit is less than 1 byte.");
		}
		
		if (options.StorageSizeLimitPerUser < options.FileSizeLimit)
		{
			sb.Append("\nStorage size limit is less than File size limit.");
		}
		
		if (options.FullPathMaxLength < options.PathSegmentMaxLength + options.FileNameMaxLength)
		{
			sb.Append("\nFull path max length is shorter than Folder + File names length.");
		}
		
		return sb.Length != 0
			? ValidateOptionsResult.Fail(sb.ToString())
			: ValidateOptionsResult.Success;
	}
}