using Microsoft.AspNetCore.Authentication.Cookies;

namespace FileStorageApi.Features.Auth.Services;

public class CookieAuthSchemeProvider : IAuthSchemeProvider
{
	public string Scheme => CookieAuthenticationDefaults.AuthenticationScheme;
}