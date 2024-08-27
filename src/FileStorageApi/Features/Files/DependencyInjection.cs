using Microsoft.Extensions.DependencyInjection;
using FileStorageApi.Features.Files.Services;

namespace FileStorageApi.Features.Files;

public static class DependencyInjection
{
	public static IServiceCollection AddFilesFeatureServices(this IServiceCollection services)
	{
		services.AddSingleton<IFileSignature, FileSignature>();
		
		return services;
	}
}