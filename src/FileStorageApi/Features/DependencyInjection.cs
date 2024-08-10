using FileStorageApi.Features.Auth;
using FileStorageApi.Features.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FileStorageApi.Features;

public static class DependencyInjection
{
	public static IServiceCollection AddFeatures(this IServiceCollection services)
	{
		services.AddRequestHandlersFromExecutingAssembly();
		
		services.AddAuthFeatureServices();
		
		return services;
	}
}