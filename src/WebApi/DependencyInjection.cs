using Microsoft.Extensions.DependencyInjection;
using WebApi.Infrastructure;

namespace WebApi;

public static class DependencyInjection
{
	public static IServiceCollection AddWebApiServices(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer().AddSwaggerGen();
		
		services.AddCookieAuthentication();
		services.AddAuthorization();
		
		services.AddHttpContextAccessor();
		
		return services;
	}
}