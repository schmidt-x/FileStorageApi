using FileStorageApi.Common.Exceptions;

namespace FileStorageApi.Features.Auth.Exceptions;

public class DuplicateEmailException : KeyValueException
{
	public DuplicateEmailException(string emailAddress)
		: base("DuplicateEmail", $"Email address '{emailAddress}' is already taken.")
	{	}
}