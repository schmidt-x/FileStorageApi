namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class InvalidPathException : FolderException
{
	public InvalidPathException(string message) : base(message)
	{	}
}