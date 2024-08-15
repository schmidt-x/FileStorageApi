using System;

namespace FileStorageApi.Domain.Entities;

public class File
{
	public Guid Id { get; set; }
	public string Name { get; set; } = default!;
	public string Extension { get; set; } = default!;
	public long Size { get; set; }
	public bool IsTrashed { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset ModifiedAt { get; set; }
	public Guid FolderId { get; set; }
	public Guid UserId { get; set; }
}