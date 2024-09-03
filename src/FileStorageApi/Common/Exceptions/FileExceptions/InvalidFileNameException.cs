namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class InvalidFileNameException : KeyValueException
{
	public InvalidFileNameException(string message) : base("InvalidFileName", message)
	{ }
}