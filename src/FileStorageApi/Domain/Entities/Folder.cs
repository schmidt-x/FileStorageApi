using System;

namespace FileStorageApi.Domain.Entities;

	public class Folder
{
	public Guid Id { get; set; }
	public string Name { get; set; } = default!;
	public string Path { get; set; } = default!;
	public long Size { get; set; }
	public bool IsTrashed { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset ModifiedAt { get; set; }
	public Guid ParentId { get; set; }
	public Guid UserId { get; set; }
}