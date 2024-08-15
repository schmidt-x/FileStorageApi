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
		
		const long _20MB = 1024 * 1024 * 20;
		
		if (options.FileSizeLimit is < 1 or > _20MB)
		{
			sb.Append("\nFile size value must be between 1 (1 byte) and 20971520 (20MB).");
		}
		
		return sb.Length != 0
			? ValidateOptionsResult.Fail(sb.ToString())
			: ValidateOptionsResult.Success;
	}
}