using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using WebApi.Infrastructure;
using System;
using App;

namespace WebApi;

class Program
{
	private static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Services
			.AddAppServices()
			.AddWebApiServices();
		
		var app = builder.Build();
		
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		
		app.MapEndpoints();
		
		app.Run();
	}
}
