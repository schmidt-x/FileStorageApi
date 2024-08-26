using System;

namespace FileStorageApi.Common.Exceptions;

public class FolderException : Exception
{
	public FolderException(string message) : base(message)
	{	}
}