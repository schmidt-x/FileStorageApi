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
using System.Security.Authentication;
using FileStorageApi.Common.Models;

namespace FileStorageApi.Endpoints;

public class Auth : EndpointGroupBase
{
	public override void Map(WebApplication app)
	{
		var auth = app
			.MapGroup("/api/auth")
			.WithTags("Auth")
			.DisableAntiforgery();  // TODO: remove
		
		auth
			.MapPost("/register", RegisterAsync)
			.Produces(StatusCodes.Status201Created)
			.Produces<FailResponse>(StatusCodes.Status422UnprocessableEntity);
		
		auth
			.MapPost("/login", LoginAsync)
			.Produces(StatusCodes.Status200OK)
			.Produces<FailResponse>(StatusCodes.Status422UnprocessableEntity);
	}
	
	public async Task<IResult> RegisterAsync(
		[FromForm] string email,
		[FromForm] string username,
		[FromForm] string password,
		CreateUserCommandHandler handler,
		HttpResponse response,
		CancellationToken ct)
	{
		Result<ClaimsPrincipal> result = await handler.Handle(new CreateUserCommand(email, username, password), ct);
		
		if (result.IsError(out Exception? ex))
		{
			return Results.UnprocessableEntity(ex is ValidationException vEx
				? new FailResponse(vEx.Errors)
				: throw ex);
		}
		
		response.StatusCode = StatusCodes.Status201Created;
		return Results.SignIn(result.Value, new AuthenticationProperties { IsPersistent = true });
	}	
	
	public async Task<IResult> LoginAsync(
		[FromForm] string login,
		[FromForm] string password,
		LoginUserCommandHandler handler,
		HttpResponse response,
		CancellationToken ct)
	{
		Result<ClaimsPrincipal> result = await handler.Handle(new LoginUserCommand(login, password), ct);
		
		if (result.IsError(out Exception? ex))
		{
			return Results.UnprocessableEntity(ex is AuthenticationException
				? new FailResponse(ErrorDetails.AuthFailure)
				: throw ex);
		}
		
		response.StatusCode = StatusCodes.Status200OK;
		return Results.SignIn(result.Value, new AuthenticationProperties { IsPersistent = true });
	}
	
}