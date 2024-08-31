using FileStorageApi.Common.Exceptions;

namespace FileStorageApi.Features.Auth.Exceptions;

public class DuplicateUsernameException : KeyValueException
{
	public DuplicateUsernameException(string username)
		: base("DuplicateUsername", $"Username '{username}' is already taken.")
	{ }
}