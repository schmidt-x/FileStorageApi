using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;

namespace FileStorageApi.Features.Infrastructure;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddRequestHandlersFromExecutingAssembly(this IServiceCollection services)
	{
		var types = Assembly
			.GetExecutingAssembly()
			.GetExportedTypes()
			.Where(t => t is { IsClass: true, IsAbstract: false} && t.IsSubclassOf(typeof(RequestHandlerBase)));
		
		foreach (var type in types)
		{
			services.AddScoped(type);
		}
		
		return services;
	}
}