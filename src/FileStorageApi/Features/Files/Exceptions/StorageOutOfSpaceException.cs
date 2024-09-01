using FileStorageApi.Common.Exceptions;

namespace FileStorageApi.Features.Files.Exceptions;

public class StorageOutOfSpaceException : KeyValueException
{
	public StorageOutOfSpaceException() : base("StorageOutOfSpace", "Not enough space left.")
	{ }
}