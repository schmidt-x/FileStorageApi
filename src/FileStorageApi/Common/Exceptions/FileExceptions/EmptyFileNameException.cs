namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class EmptyFileNameException : KeyValueException
{
	public EmptyFileNameException() : base("EmptyFileName", "FileName is empty.")
	{	}
}