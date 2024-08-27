using Microsoft.Extensions.DependencyInjection;
using FileStorageApi.Features.Infrastructure;
using FileStorageApi.Features.Files;
using FileStorageApi.Features.Auth;

namespace FileStorageApi.Features;

public static class DependencyInjection
{
	public static IServiceCollection AddFeatures(this IServiceCollection services)
	{
		services.AddRequestHandlersFromExecutingAssembly();
		
		services.AddAuthFeatureServices();
		services.AddFilesFeatureServices();
		
		return services;
	}
}