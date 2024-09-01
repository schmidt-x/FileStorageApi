namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class InvalidFileExtensionException : KeyValueException
{
	public InvalidFileExtensionException(string message)
		: base("InvalidFileExtension", message)
	{	}
}