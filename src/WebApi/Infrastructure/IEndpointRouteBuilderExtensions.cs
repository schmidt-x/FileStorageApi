using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace WebApi.Infrastructure;

public static class IEndpointRouteBuilderExtensions
{
	public static RouteHandlerBuilder MapGet(
		this IEndpointRouteBuilder builder, 
		Delegate handler, 
		[StringSyntax("Route")] string pattern = "")
	{
		return builder
			.MapGet(pattern, handler)
			.WithName(handler.Method.Name);
	}
	
	public static RouteHandlerBuilder MapPost(
		this IEndpointRouteBuilder builder, 
		Delegate handler, 
		[StringSyntax("Route")] string pattern = "")
	{
		return builder
			.MapPost(pattern, handler)
			.WithName(handler.Method.Name);
	}
	
	public static RouteHandlerBuilder MapPatch(
		this IEndpointRouteBuilder builder, 
		Delegate handler, 
		[StringSyntax("Route")] string pattern = "")
	{
		return builder
			.MapPatch(pattern, handler)
			.WithName(handler.Method.Name);
	}
	
	public static RouteHandlerBuilder MapDelete(
		this IEndpointRouteBuilder builder, 
		Delegate handler, 
		[StringSyntax("Route")] string pattern = "")
	{
		return builder
			.MapDelete(pattern, handler)
			.WithName(handler.Method.Name);
	}
}