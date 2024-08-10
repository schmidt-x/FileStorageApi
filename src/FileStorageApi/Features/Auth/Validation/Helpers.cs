namespace FileStorageApi.Features.Auth.Validation;

public static class Helpers
{
	public static string[] ValidatePassword(string password)
	{
		bool hasLower = false;
		bool hasUpper = false;
		bool hasNumber = false;
		bool hasSymbol = false;
		bool hasWhiteSpace = false;
		
		int errors = 4;
		
		foreach (var l in password)
		{
			if (!hasLower && char.IsLower(l))
			{
				hasLower = true;
				errors--;
			}
			else if (!hasUpper && char.IsUpper(l))
			{
				hasUpper = true;
				errors--;
			}
			else if (!hasNumber && char.IsNumber(l))
			{
				hasNumber = true;
				errors--;
			}
			else if (!hasSymbol && (char.IsSymbol(l) || char.IsPunctuation(l)))
			{
				hasSymbol = true;
				errors--;
			}
			else if (!hasWhiteSpace && char.IsWhiteSpace(l))
			{
				hasWhiteSpace = true;
				errors++;
			}
			
			if (errors == 1 && hasWhiteSpace)
			{
				break;
			}
		}
		
		if (errors == 0)
		{
			return [];
		}
		
		var failures = new string[errors];
		int i = 0;
		
		if (!hasLower)      failures[i++] = "Password must contain at least one lower-case character.";
		if (!hasUpper)      failures[i++] = "Password must contain at least one upper-case character.";
		if (!hasNumber)     failures[i++] = "Password must contain at least one number.";
		if (!hasSymbol)     failures[i++] = "Password must contain at least one symbol.";
		if (hasWhiteSpace)  failures[i]   = "Password must not contain any white spaces.";
		
		return failures;
	}

}