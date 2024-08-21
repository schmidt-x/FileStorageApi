using System;

namespace FileStorageApi.Domain.Entities;

public class Folder
{
	public Guid Id { get; init; }
	public string Name { get; init; } = default!;
	public Guid PathId { get; init; }
	public long Size { get; init; }
	public bool IsTrashed { get; init; }
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset ModifiedAt { get; init; }
	public Guid ParentId { get; init; }
	public Guid UserId { get; init; }
	public FolderPath? FolderPath { get; init; }
}