using System;

namespace FileStorageApi.Common.Exceptions.FolderExceptions;

public class FolderException : Exception
{
	public FolderException(string message) : base(message)
	{	}
}