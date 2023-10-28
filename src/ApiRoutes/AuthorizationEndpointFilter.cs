using Microsoft.AspNetCore.Builder;

namespace ApiRoutes;

public sealed class AuthorizationEndpointFilter : IEndpointFilter
{
    public void Handle(RouteMetadata metadata, IEndpointConventionBuilder route)
    {
        if (metadata.RequiresAuth)
        {
            if (string.IsNullOrEmpty(metadata.AuthPolicy))
            {
                route.RequireAuthorization();
            }
            else
            {
                route.RequireAuthorization(metadata.AuthPolicy);
            }
        }
    }
}