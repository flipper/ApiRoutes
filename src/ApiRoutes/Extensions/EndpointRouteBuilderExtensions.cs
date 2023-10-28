using JetBrains.Annotations;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ApiRoutes;

[PublicAPI]
public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiRoutes(this IEndpointRouteBuilder app)
    {
        var configurations = app.ServiceProvider.GetServices<IRouteConfiguration>();

        foreach (var configuration in configurations)
        {
            configuration.MapRoutes(app);
        }

        return app;
    }
}