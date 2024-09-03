using System.IO;
using System;

namespace FileStorageApi.Features.Files.Helpers;

public static class FilePathGenerator
{
	public static string Generate(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
			throw new ArgumentNullException(nameof(fileName));
		
		const int charactersLength = 4;
		
		if (fileName.Length < charactersLength)
			throw new InvalidOperationException(
				$"The 'fileName' must be at least {charactersLength} characters long, but got {fileName.Length}.");
		
		return $"{fileName[..2]}{Path.DirectorySeparatorChar}{fileName[2..4]}";
	}
}