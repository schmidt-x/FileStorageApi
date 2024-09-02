using FileStorageApi.Common.Exceptions;

namespace FileStorageApi.Features.Files.Exceptions;

public class InvalidFileExtensionException : KeyValueException
{
	public InvalidFileExtensionException(string message)
		: base("InvalidFileExtension", message)
	{	}
}