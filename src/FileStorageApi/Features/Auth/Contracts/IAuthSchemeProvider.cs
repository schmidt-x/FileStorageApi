namespace FileStorageApi.Features.Auth.Contracts;

public interface IAuthSchemeProvider
{
	string Scheme { get; }
}