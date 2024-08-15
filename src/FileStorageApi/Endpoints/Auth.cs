using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using FileStorageApi.Features.Auth.Commands.CreateUser;
using FileStorageApi.Features.Auth.Commands.LoginUser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using FileStorageApi.Infrastructure;
using FileStorageApi.Common.Exceptions;
using System.Threading;
using FileStorageApi.Responses;
using FileStorageApi.Common;
using System;

namespace FileStorageApi.Endpoints;

public class Auth : EndpointGroupBase
{
	public async Task<IResult> RegisterAsync(
		[FromForm] string email,
		[FromForm] string username,
		[FromForm] string password,
		CreateUserHandler handler,
		IHttpContextAccessor ctxAccessor,
		CancellationToken ct)
	{
		var command = new CreateUserCommand(email, username, password);
		
		Result<ClaimsPrincipal> result = await handler.Handle(command, ct);
		
		if (result.IsError(out Exception? ex))
		{
			return Results.BadRequest(ex is ValidationException validationEx
				? new FailResponse(validationEx.Errors)
				: new FailResponse("auth", ex.Message));
		}
		
		var ctx = ctxAccessor.HttpContext!;
		
		await ctx.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			result.Value,
			new AuthenticationProperties { IsPersistent = true });
		
		return Results.Created();
	}	
	
	public async Task<IResult> LoginAsync(
		[FromForm] string login,
		[FromForm] string password,
		LoginUser handler,
		IHttpContextAccessor ctxAccessor,
		CancellationToken ct)
	{
		var command = new LoginUserCommand(login, password);
		
		Result<ClaimsPrincipal> result = await handler.Handle(command, ct);
		
		if (result.IsError(out Exception? ex))
		{
			return Results.BadRequest(ex is ValidationException validationEx
				? new FailResponse(validationEx.Errors)
				: new FailResponse("auth", ex.Message));
		}
		
		var ctx = ctxAccessor.HttpContext!;
		
		await ctx.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			result.Value,
			new AuthenticationProperties { IsPersistent = true });
		
		return Results.NoContent();
	}
	
	public override void Map(WebApplication app)
	{
		var auth = app
			.MapGroup("/api/auth")
			.WithTags("Auth")
			.DisableAntiforgery();  // TODO: remove
		
		auth
			.MapPost("/register", RegisterAsync)
			.Produces(StatusCodes.Status201Created)
			.Produces<FailResponse>(StatusCodes.Status400BadRequest);
		
		auth
			.MapPost("/login", LoginAsync)
			.Produces(StatusCodes.Status204NoContent)
			.Produces<FailResponse>(StatusCodes.Status400BadRequest);
			
	}
}