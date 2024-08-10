using BCryptHasher = BCrypt.Net.BCrypt;

using System;

namespace FileStorageApi.Features.Auth.Services;

public class PasswordHasher : IPasswordHasher
{
	public string Hash(string password)
	{
		if (string.IsNullOrWhiteSpace(password))
			throw new ArgumentNullException(nameof(password));
		
		return BCryptHasher.HashPassword(password);
	}
	
	public bool Verify(string passwordHash, string password)
	{
		if (string.IsNullOrWhiteSpace(passwordHash))
			throw new ArgumentNullException(nameof(passwordHash));
		
		if (string.IsNullOrWhiteSpace(password))
			throw new ArgumentNullException(nameof(password));
		
		return BCryptHasher.Verify(password, passwordHash);
	}
}