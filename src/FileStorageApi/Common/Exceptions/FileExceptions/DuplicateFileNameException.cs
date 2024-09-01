namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class DuplicateFileNameException : KeyValueException
{
	public DuplicateFileNameException(string fileName)
		: base("DuplicateFileName", $"File with the name '{fileName}' already exists.")
	{	}
}