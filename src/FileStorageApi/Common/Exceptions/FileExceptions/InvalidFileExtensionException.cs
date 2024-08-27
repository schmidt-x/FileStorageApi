namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class InvalidFileExtensionException : FileException
{
	public InvalidFileExtensionException(string message) : base(message)
	{	}
}