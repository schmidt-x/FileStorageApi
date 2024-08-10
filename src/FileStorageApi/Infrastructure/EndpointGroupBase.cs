using Microsoft.AspNetCore.Builder;

namespace FileStorageApi.Infrastructure;

public abstract class EndpointGroupBase
{
	public abstract void Map(WebApplication app);
}