namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class DuplicateFileNameException : FileException
{
	public DuplicateFileNameException(string message) : base(message)
	{	}
}