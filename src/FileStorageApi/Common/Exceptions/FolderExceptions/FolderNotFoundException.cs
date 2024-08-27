namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class FolderNotFoundException : FolderException
{
	public FolderNotFoundException(string message) : base(message)
	{	}
}