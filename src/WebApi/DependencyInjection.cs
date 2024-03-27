using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace WebApi;

public static class DependencyInjection
{
	public static IServiceCollection AddWebApiServices(this IServiceCollection services)
	{
		// services.Configure<ApiBehaviorOptions>(options =>
			// options.SuppressModelStateInvalidFilter = true);
		
		services.AddEndpointsApiExplorer();
		
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo { Title = "FileStorageApi", Version = "v1" });
		});
		
		return services;
	}
}