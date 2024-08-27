using System;

namespace FileStorageApi.Common.Exceptions.FileExceptions;

public class FileException : Exception
{
	public FileException(string message) : base(message)
	{ }
}