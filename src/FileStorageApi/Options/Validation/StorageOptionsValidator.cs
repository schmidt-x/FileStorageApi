using Microsoft.Extensions.Options;
using System.Text;

namespace FileStorageApi.Options.Validation;

public class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
	public ValidateOptionsResult Validate(string? name, StorageOptions options)
	{
		var sb = new StringBuilder();
				
		if (string.IsNullOrWhiteSpace(options.RootFolder))
		{
			sb.Append("\nRootFolder is required.");
		}
		
		if (string.IsNullOrWhiteSpace(options.StorageFolder))
		{
			sb.Append("\nStorageFolder is required.");
		}
		
		if (string.IsNullOrWhiteSpace(options.TrashFolder))
		{
			sb.Append("\nTrashFolder is required.");
		}
		
		return sb.Length != 0
			? ValidateOptionsResult.Fail(sb.ToString())
			: ValidateOptionsResult.Success;
	}
}