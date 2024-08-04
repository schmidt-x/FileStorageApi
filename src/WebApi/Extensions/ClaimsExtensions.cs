using System.Collections.Generic;
using System.Security.Claims;

namespace WebApi.Extensions;

public static class ClaimsExtensions
{
	public static ClaimsPrincipal ToPrincipal(this IEnumerable<Claim> claims, string authScheme)
	{
		return new ClaimsPrincipal(new ClaimsIdentity(claims, authScheme));
	}
}