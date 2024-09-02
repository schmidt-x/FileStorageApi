namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class FolderNameTooLongException : KeyValueException
{
	public FolderNameTooLongException(int limit)
		: base("FolderNameTooLong", $"FolderName's length exceeds the limit of {limit} characters.")
	{	}
}