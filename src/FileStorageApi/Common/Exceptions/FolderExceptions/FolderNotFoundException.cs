namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class FolderNotFoundException : KeyValueException
{
	public FolderNotFoundException(string folderName)
		: base("FolderNotFound", $"Folder '{folderName}' does not exist.")
	{	}
}