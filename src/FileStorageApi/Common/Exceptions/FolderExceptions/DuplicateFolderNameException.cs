namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class DuplicateFolderNameException : KeyValueException
{
	public DuplicateFolderNameException(string folderName)
		: base("DuplicateFolderName", $"Folder '{folderName}' already exists.")
	{	}
}