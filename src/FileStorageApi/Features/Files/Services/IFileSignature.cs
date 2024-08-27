using FileStorageApi.Domain.Enums;
using FileStorageApi.Common;
using System.IO;

namespace FileStorageApi.Features.Files.Services;

public interface IFileSignature
{
	Result<FileType> Validate(Stream file, string ext, string mimeType);
}