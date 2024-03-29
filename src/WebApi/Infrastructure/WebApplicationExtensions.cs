﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace WebApi.Infrastructure;

public static class WebApplicationExtensions
{
	public static WebApplication MapEndpoints(this WebApplication app)
	{
		var assembly = Assembly.GetExecutingAssembly();
		
		var endpointGroupTypes = assembly
			.GetExportedTypes()
			.Where(t => t.IsSubclassOf(typeof(EndpointGroupBase)));
		
		foreach (var type in endpointGroupTypes)
		{
			if (Activator.CreateInstance(type) is EndpointGroupBase instance)
			{
				instance.Map(app);
			}
		}
		
		return app;
	}
}