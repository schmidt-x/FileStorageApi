using FileStorageApi.Features.Auth.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FileStorageApi.Services;

public class CookieAuthSchemeProvider : IAuthSchemeProvider
{
	public string Scheme => CookieAuthenticationDefaults.AuthenticationScheme;
}