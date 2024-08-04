using System;

namespace App.Services;

public class PasswordHasher : IPasswordHasher
{
	public string Hash(string password)
	{
		if (string.IsNullOrWhiteSpace(password))
			throw new ArgumentNullException(nameof(password));
		
		return BCrypt.Net.BCrypt.HashPassword(password);
	}
	
	public bool Verify(string passwordHash, string password)
	{
		if (string.IsNullOrWhiteSpace(passwordHash))
			throw new ArgumentNullException(nameof(passwordHash));
		
		if (string.IsNullOrWhiteSpace(password))
			throw new ArgumentNullException(nameof(password));
		
		return BCrypt.Net.BCrypt.Verify(password, passwordHash);
	}
}