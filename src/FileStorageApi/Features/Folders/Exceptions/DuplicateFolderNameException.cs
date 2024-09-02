using FileStorageApi.Common.Exceptions;

namespace FileStorageApi.Features.Folders.Exceptions;

public class DuplicateFolderNameException : KeyValueException
{
	public DuplicateFolderNameException(string folderName)
		: base("DuplicateFolderName", $"Folder '{folderName}' already exists.")
	{	}
}