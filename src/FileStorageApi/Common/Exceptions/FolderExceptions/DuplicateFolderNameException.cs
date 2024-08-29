using System;

namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class DuplicateFolderNameException : Exception
{
	public DuplicateFolderNameException(string message) : base(message)
	{	}
}