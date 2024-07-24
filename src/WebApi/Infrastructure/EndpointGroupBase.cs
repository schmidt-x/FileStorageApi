using Microsoft.AspNetCore.Builder;

namespace WebApi.Infrastructure;

public abstract class EndpointGroupBase
{
	public abstract void Map(WebApplication app);
}