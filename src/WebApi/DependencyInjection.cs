using Microsoft.Extensions.DependencyInjection;

namespace WebApi;

public static class DependencyInjection
{
	public static IServiceCollection AddWebApiServices(this IServiceCollection services)
	{
		services
			.AddEndpointsApiExplorer()
			.AddSwaggerGen();
		
		return services;
	}
}