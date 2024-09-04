using FileStorageApi.Common.Exceptions;

namespace FileStorageApi.Features.Files.Exceptions;

public class FileNotFoundException : KeyValueException
{
	public FileNotFoundException(string fileName)
		: base("FileNotFound", $"File with the name '{fileName}' does not exist.")
	{	}
}