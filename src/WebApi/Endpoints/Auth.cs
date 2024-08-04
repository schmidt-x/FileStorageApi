using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using App.Features.Auth.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using WebApi.Infrastructure;
using App.Common.Exceptions;
using WebApi.Extensions;
using System.Threading;
using WebApi.Responses;
using App.Common;
using System;

namespace WebApi.Endpoints;

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
		
		Result<IEnumerable<Claim>> result = await handler.Handle(command, ct);
		
		if (result.IsError(out Exception? ex))
		{
			return Results.BadRequest(ex is ValidationException validationEx
				? new FailResponse(validationEx.Errors)
				: new FailResponse("error", ex.Message));
		}
		
		var ctx = ctxAccessor.HttpContext!;
		var claims = result.Value;
		
		await ctx.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			claims.ToPrincipal(CookieAuthenticationDefaults.AuthenticationScheme),
			new AuthenticationProperties { IsPersistent = true });
		
		return Results.Created();
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
		
	}
}