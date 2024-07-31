using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebApi.Infrastructure;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddCookieAuthentication(this IServiceCollection services)
	{
		services
			.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
			{
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				
				options.Events.OnRedirectToLogin = rc =>
				{
					rc.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return Task.CompletedTask;
				};
				
				options.Events.OnRedirectToAccessDenied = rc =>
				{
					rc.Response.StatusCode = StatusCodes.Status403Forbidden;
					return Task.CompletedTask;
				};
			});
		
		return services;
	}
}