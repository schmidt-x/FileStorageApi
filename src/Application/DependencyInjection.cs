using Microsoft.Extensions.DependencyInjection;

namespace FileStorageApi.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		return services;
	}
}