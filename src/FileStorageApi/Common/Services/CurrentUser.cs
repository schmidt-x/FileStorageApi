using FileStorageApi.Domain.Constants;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security;
using System;

namespace FileStorageApi.Common.Services;

public class CurrentUser : IUser
{
	private readonly IHttpContextAccessor _ctxAccessor;
	
	public CurrentUser(IHttpContextAccessor ctxAccessor)
	{
		_ctxAccessor = ctxAccessor;
	}
	
	public Guid Id()
	{
		var user = _ctxAccessor.HttpContext!.User;
		
		ThrowIfNotAuthenticated(user);
		
		var rawId = user.FindFirstValue(ClaimTypes.NameIdentifier) 
		            ?? throw new SecurityException($"Claim '{nameof(ClaimTypes.NameIdentifier)}' is not present.");
		
		if (!Guid.TryParse(rawId, out var id))
		{
			throw new SecurityException($"Claim '{nameof(ClaimTypes.NameIdentifier)}' is not valid Guid value.");
		}
		
		return id;
	}
	
	public Guid FolderId()
	{
		var user = _ctxAccessor.HttpContext!.User;
		
		ThrowIfNotAuthenticated(user);
		
		var rawId = user.FindFirstValue(AuthClaims.FolderId) 
		            ?? throw new SecurityException($"Claim '{nameof(AuthClaims.FolderId)}' is not present.");
		
		if (!Guid.TryParse(rawId, out var id))
		{
			throw new SecurityException($"Claim '{nameof(AuthClaims.FolderId)}' is not valid Guid value.");
		}
		
		return id;
	}
	
	private static void ThrowIfNotAuthenticated(ClaimsPrincipal user)
	{
		if (user.Identity is null || !user.Identity.IsAuthenticated)
		{
			throw new InvalidOperationException("User is not authenticated.");
		}
	}
}