namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class InvalidPathException : KeyValueException
{
	public InvalidPathException(string message)	: base("InvalidPath", message)
	{	}
}