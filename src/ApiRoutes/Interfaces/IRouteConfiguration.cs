using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ApiRoutes;

public interface IRouteConfiguration
{
    Dictionary<(string, Method), RouteMetadata> Routes { get; }

    void MapRoutes(IEndpointRouteBuilder app);

    void RegisterServices(IServiceCollection services);
}