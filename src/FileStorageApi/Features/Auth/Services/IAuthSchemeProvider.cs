namespace FileStorageApi.Features.Auth.Services;

public interface IAuthSchemeProvider
{
	string Scheme { get; }
}