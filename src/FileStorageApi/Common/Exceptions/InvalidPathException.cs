namespace FileStorageApi.Common.Exceptions;

public class InvalidPathException : FolderException
{
	public InvalidPathException(string message) : base(message)
	{	}
}