using Microsoft.AspNetCore.Builder;

namespace ApiRoutes.Swagger;

public class SwaggerEndpointFilter : IEndpointFilter
{
    public void Handle(RouteMetadata metadata, IEndpointConventionBuilder route)
    {
        route
            .WithName(metadata.Request.FullName);
    }
}