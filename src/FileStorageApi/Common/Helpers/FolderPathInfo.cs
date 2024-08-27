using FileStorageApi.Common.Exceptions.FolderExceptions;
using FileStorageApi.Common.Extensions;
using System.Diagnostics.CodeAnalysis;
using System;

namespace FileStorageApi.Common.Helpers;

public class FolderPathInfo
{
	public string Path { get; }
	public string Name { get; }
	
	public static readonly char[] PathSeparators = ['\\', '/'];
	
	private FolderPathInfo(string path, string name)
	{
		Path = path;
		Name = name;
	}
	
	public static Result<FolderPathInfo> New(string folderPath, int pathSegmentMaxLength = int.MaxValue)
	{
		if (string.IsNullOrEmpty(folderPath))
		{
			return new InvalidPathException("Folder path is empty.");
		}
		
		var trimmedPath = folderPath.AsSpan().Trim();
		
		if (trimmedPath.Length == 1 && trimmedPath[0] is '/')
		{
			return new FolderPathInfo("", "/");
		}
		
		var segments = folderPath.Split(
			PathSeparators,
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		
		return segments.Length switch
		{
			0 => new InvalidPathException("Invalid folder path."),
			1 => ValidateSingle(segments[0], pathSegmentMaxLength),
			_ => Validate(segments, pathSegmentMaxLength)
		};
	}
	
	public bool TryGetParent([MaybeNullWhen(false)] out FolderPathInfo parent)
	{
		parent = null;
		
		switch (Path)
		{
			case "":
				return false;
			case "/":
				parent = new FolderPathInfo("", "/");
				return true;
		}
		
		// Get rid of the separator at the end. 
		var currentPath = Path.AsSpan(..^1);
		
		var index = currentPath.LastIndexOf('/') + 1;
		if (index == 0)
		{
			return false;
		}
		
		var path = currentPath[..index].ToString();
		var name = currentPath[index..].ToString();
		
		parent = new FolderPathInfo(path, name);
		return true;
	}
	
	private static Result<FolderPathInfo> ValidateSingle(string segment, int pathSegmentMaxLength)
	{
		if (segment.ContainsControlChars())
		{
			return new InvalidPathException("Folder name contains Control characters.");
		}
		
		if (segment.Length > pathSegmentMaxLength)
		{
			return new InvalidPathException($"Folder name exceeds the limit of {pathSegmentMaxLength} characters.");
		}
		
		return new FolderPathInfo("/", segment);
	}
	
	private static Result<FolderPathInfo> Validate(string[] segments, int pathSegmentMaxLength)
	{
		foreach (var segment in segments)
		{
			if (segment.ContainsControlChars())
			{
				return new InvalidPathException("Folder name contains Control characters.");
			}
			
			if (segment.Length > pathSegmentMaxLength)
			{
				return new InvalidPathException($"Folder name exceeds the limit of {pathSegmentMaxLength} characters.");
			}
		}
		
		var path = $"/{string.Join('/', segments[..^1])}/";
		var name = segments[^1];
		
		return new FolderPathInfo(path, name);
	}
}