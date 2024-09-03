namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class FileNameTooLongException : KeyValueException
{
	public FileNameTooLongException(int limit)
		: base("FileNameTooLong", $"FileName's length exceeds the limit of {limit} characters.")
	{	}
}