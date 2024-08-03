using System;

namespace App.Domain.Entities;

public class User
{
	public Guid Id { get; init; }
	public string Email { get; init; } = default!;
	public bool IsConfirmed { get; init; }
	public string Username { get; init; } = default!;
	public string PasswordHash { get; init; } = default!;
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset ModifiedAt { get; init; }
	public Guid FolderId { get; init; }
}