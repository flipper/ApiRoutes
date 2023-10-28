using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace ApiRoutes;

[PublicAPI]
public interface IEndpointFilter
{
    void Handle(RouteMetadata metadata, IEndpointConventionBuilder route);
}