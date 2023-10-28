using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ApiRoutes;

public class ApiRouteConfiguration
{
    public ApiRouteConfiguration(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public List<IRouteConfiguration> RouteConfigurations { get; } = new();

    internal readonly List<Action<ApiRouteConfiguration>> _postConfigurators = new();

    [PublicAPI]
    public ApiRouteConfiguration PostConfigure(Action<ApiRouteConfiguration> configure)
    {
        _postConfigurators.Add(configure);
        return this;
    }
}