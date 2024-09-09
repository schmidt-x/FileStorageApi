using FileStorageApi.Features.Files.Exceptions;
using FileStorageApi.Domain.Enums;
using System.Collections.Generic;
using FileStorageApi.Common;
using System.IO;
using System;

namespace FileStorageApi.Features.Files.Services;

public class FileSignature : IFileSignature
{
	private record FileInfo(string MimeType, FileType FileType /*, TODO: FileSignature */);
	
	private static readonly Dictionary<string, FileInfo> FileExtensions = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "jpg",  new FileInfo("image/jpeg", FileType.Image) },
		{ "jpeg", new FileInfo("image/jpeg", FileType.Image) },
		{ "png",  new FileInfo("image/png", FileType.Image) },
		{ "gif",  new FileInfo("image/gif", FileType.Image) },
		{ "webp", new FileInfo("image/webp", FileType.Image) },
		
		{ "mp3",  new FileInfo("audio/mpeg", FileType.Audio) },
		{ "wav",  new FileInfo("audio/wav", FileType.Audio) },
		{ "aac",  new FileInfo("audio/aac", FileType.Audio) },
		{ "ogg",  new FileInfo("audio/ogg", FileType.Audio) },
		{ "weba", new FileInfo("audio/webm", FileType.Audio) },
		{ "mid",  new FileInfo("audio/midi", FileType.Audio) },
		{ "midi", new FileInfo("audio/x-midi", FileType.Audio) },
		{ "oga",  new FileInfo("audio/ogg", FileType.Audio) },
		{ "opus", new FileInfo("audio/ogg", FileType.Audio) },
		
		{ "mp4",  new FileInfo("video/mp4", FileType.Video) },
		{ "avi",  new FileInfo("video/x-msvideo", FileType.Video) },
		{ "mov",  new FileInfo("video/quicktime", FileType.Video) },
		{ "flv",  new FileInfo("video/x-flv", FileType.Video) },
		{ "wmv",  new FileInfo("video/x-ms-wmv", FileType.Video) },
		{ "webm", new FileInfo("video/webm", FileType.Video) },
		
		{ "pdf",  new FileInfo("application/pdf", FileType.Document) },
		{ "json", new FileInfo("application/json", FileType.Document) },
		{ "txt",  new FileInfo("text/plain", FileType.Document) },
		{ "html", new FileInfo("text/html", FileType.Document) },
		{ "xml",  new FileInfo("application/xml", FileType.Document) },
		{ "xls",  new FileInfo("application/vnd.ms-excel", FileType.Document) },
		{ "xlsx", new FileInfo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileType.Document) },
		{ "ppt",  new FileInfo("application/vnd.ms-powerpoint", FileType.Document) },
		{ "pptx", new FileInfo("application/vnd.openxmlformats-officedocument.presentationml.presentation", FileType.Document) },
		{ "doc",  new FileInfo("application/msword", FileType.Document) },
		{ "docx", new FileInfo("application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileType.Document) },
		
		{ "zip", new FileInfo("application/zip", FileType.Archive) },
		{ "rar", new FileInfo("application/vnd.rar", FileType.Archive) },
		{ "7z",  new FileInfo("application/x-7z-compressed", FileType.Archive) },
		{ "bz",  new FileInfo("application/x-bzip", FileType.Archive) },
		{ "bz2", new FileInfo("application/x-bzip2", FileType.Archive) },
		{ "gz",  new FileInfo("application/gzip", FileType.Archive) },
		{ "jar", new FileInfo("application/java-archive", FileType.Archive) },
		{ "tar", new FileInfo("application/x-tar", FileType.Archive) }
	};

	
	public Result<FileType> Validate(Stream file, string ext, string mimeType)
	{
		// If the file extension is known, validate its Mime-type and Signatures, and return an appropriate FileType.
		// If not, skip the validation and return 'FileType.Unknown'.
		// On file downloading, any file that has 'Unknown' flag, will have default 'application/octet-stream' Mime-type.
		
		var fileType = FileType.Unknown;
		
		if (string.IsNullOrEmpty(ext) || file.Length == 0)
		{
			return fileType;
		}
		
		if (ext.StartsWith('.'))
		{
			ext = ext[1..];
		}
		
		if (!FileExtensions.TryGetValue(ext, out var fileInfo))
		{
			return fileType;
		}
		
		if (!mimeType.Equals(fileInfo.MimeType, StringComparison.OrdinalIgnoreCase))
		{
			return new InvalidFileExtensionException("File extension and Mime-type do not correspond to each other.");
		}
		
		// TODO: validate Magic numbers (https://www.filesignatures.net/)
		
		fileType = fileInfo.FileType;
		
		return fileType;
	}
	
	public string? GetMimeType(string extension)
	{
		if (string.IsNullOrEmpty(extension))
		{
			return null;
		}
		
		if (extension.StartsWith('.'))
		{
			extension = extension[1..];
		}
		
		_ = FileExtensions.TryGetValue(extension, out var value);
		return value?.MimeType;
	}
}