using FileStorageApi.Common.Exceptions.FileExceptions;
using FileStorageApi.Common.Extensions;
using System;

namespace FileStorageApi.Common.Helpers;

public class FilePathInfo
{
	public string Name { get; }
	public string Extension { get; }
	public string NameWithExtension { get; }
	
	private string? _fullName;
	
	public string FullName => _fullName ??= Folder.IsRootFolder
		? Folder.Name + NameWithExtension
		: Folder.FullName + "/" + NameWithExtension;
	
	public FolderPathInfo Folder { get; }
	
	private FilePathInfo(string name, string? fileExtension, FolderPathInfo folderInfo)
	{
		Name = name;
		Extension = fileExtension ?? "";
		NameWithExtension = Name + Extension;
		Folder = folderInfo;
	}
	
	public static Result<FilePathInfo> New(
		string fileName,
		int pathSegmentMaxLength = int.MaxValue,
		int fileNameMaxLength = int.MaxValue)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return new EmptyFileNameException();
		}
		
		var fileNameSpan = fileName.AsSpan().Trim();
		int sepIndex = fileNameSpan.LastIndexOfAny(FolderPathInfo.PathSeparators);
		
		var name = fileNameSpan[(sepIndex+1)..].TrimStart();
		
		var exception = IsValidName(name, fileNameMaxLength);
		if (exception is not null)
		{
			return exception;
		}
		
		string? ext = null;
		if (FileNameHasExtension(name, out var index))
		{
			ext = name[index..].ToString();
			name = name[..index].TrimEnd();
		}
		
		var path = sepIndex != -1
			? fileNameSpan[..sepIndex].ToString()
			: string.Empty;
		
		var folderInfoResult = FolderPathInfo.New(path, pathSegmentMaxLength);
		if (folderInfoResult.IsError(out var ex))
		{
			return ex;
		}
		
		return new FilePathInfo(name.ToString(), ext, folderInfoResult.Value);
	}
	
	private static bool FileNameHasExtension(ReadOnlySpan<char> fileName, out int index)
	{
		var dotIndex = fileName.LastIndexOf('.');
		 
		if (dotIndex != -1 && dotIndex != fileName.Length-1)
		{
			index = dotIndex;
			return true;
		}
		
		index = -1;
		return false;
	}
	
	private static Exception? IsValidName(ReadOnlySpan<char> name, int fileNameMaxLength)
	{
		if (name.IsEmpty)
		{
			return new EmptyFileNameException();
		}
		
		if (name.Length > fileNameMaxLength)
		{
			return new FileNameTooLongException(fileNameMaxLength);	
		}
		
		if (name.ContainsControlChars())
		{
			return new InvalidFileNameException("FileName contains Control characters.");
		}
		
		return null;
	}
}