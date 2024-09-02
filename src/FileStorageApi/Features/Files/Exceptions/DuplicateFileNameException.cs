using FileStorageApi.Common.Exceptions;

namespace FileStorageApi.Features.Files.Exceptions;

public class DuplicateFileNameException : KeyValueException
{
	public DuplicateFileNameException(string fileName)
		: base("DuplicateFileName", $"File with the name '{fileName}' already exists.")
	{	}
}