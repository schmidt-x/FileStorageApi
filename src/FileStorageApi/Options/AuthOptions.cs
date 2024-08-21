namespace FileStorageApi.Options;

public class AuthOptions
{
	public const string Auth = "Auth";
	
	public int UsernameMinLength { get; init; }
	public int UsernameMaxLength { get; init; }
	public int PasswordMinLength { get; init; }
}