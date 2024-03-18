using Microsoft.Extensions.DependencyInjection;

namespace FileStorageApi.WebApi;

public static class DependencyException
{
	public static IServiceCollection AddWebApiServices(this IServiceCollection services)
	{
		services
			.AddSwaggerGen()
			.AddEndpointsApiExplorer();
		
		return services;
	}
}