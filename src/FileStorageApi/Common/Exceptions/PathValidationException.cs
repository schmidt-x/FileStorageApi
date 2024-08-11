using System;

namespace FileStorageApi.Common.Exceptions;

public class PathValidationException : Exception
{
	public PathValidationException(string message) : base(message)
	{	}
}