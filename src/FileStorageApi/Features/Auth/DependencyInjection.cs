using Microsoft.Extensions.DependencyInjection;
using FileStorageApi.Features.Auth.Services;

namespace FileStorageApi.Features.Auth;

public static class DependencyInjection
{
	public static IServiceCollection AddAuthFeatureServices(this IServiceCollection services)
	{
		services.AddSingleton<IAuthSchemeProvider, CookieAuthSchemeProvider>();
		services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
		
		return services;
	}
}