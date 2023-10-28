using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ApiRoutes;

[PublicAPI]
public static class ApiRouteConfigurationExtensions
{
    public static ApiRouteConfiguration AddRouteConfiguration<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this ApiRouteConfiguration configuration) where T : class, IRouteConfiguration, new()
    {
        IRouteConfiguration config = new T();
        configuration.RouteConfigurations.Add(config);
        configuration.Services.AddSingleton<T>();
        configuration.Services.AddSingleton<IRouteConfiguration, T>(provider => provider.GetRequiredService<T>());

        config.RegisterServices(configuration.Services);
        return configuration;
    }
}