using FileStorageApi.Common.Contracts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security;
using System;

namespace FileStorageApi.Services;

public class CurrentUser : IUser
{
	private readonly IHttpContextAccessor _ctxAccessor;
	
	public CurrentUser(IHttpContextAccessor ctxAccessor)
	{
		_ctxAccessor = ctxAccessor;
	}
	
	public Guid Id()
	{
		var ctx = _ctxAccessor.HttpContext!;
		
		if (ctx.User.Identity is null || !ctx.User.Identity.IsAuthenticated)
		{
			throw new InvalidOperationException("User is not authenticated.");
		}
		
		var rawId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier) 
		            ?? throw new SecurityException("Claim 'NameIdentifier' is not present.");
		
		if (!Guid.TryParse(rawId, out var id))
		{
			throw new SecurityException("Claim 'NameIdentifier' is not valid Guid value.");
		}
		
		return id;
	}
}