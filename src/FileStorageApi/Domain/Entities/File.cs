using FileStorageApi.Domain.Enums;
using System;

namespace FileStorageApi.Domain.Entities;

public class File
{
	public Guid Id { get; init; }
	public string Name { get; init; } = default!;
	public string Extension { get; set; } = default!;
	public long Size { get; init; }
	public FileType Type { get; init; }
	public bool IsTrashed { get; init; }
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset ModifiedAt { get; init; }
	public Guid FolderId { get; init; }
	public Guid UserId { get; init; }
}