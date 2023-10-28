using System.Diagnostics.CodeAnalysis;
using ApiRoutes.Authentication;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ApiRoutes;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointFilter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TEndpointFilter>(
        this IServiceCollection services) where TEndpointFilter : class, IEndpointFilter
    {
        return services.AddSingleton<IEndpointFilter, TEndpointFilter>();
    }

    public static IServiceCollection AddApiRoutes<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TRouteConfiguration>(
        this IServiceCollection services,
        Action<ApiRouteConfiguration>? configure = null) where TRouteConfiguration : class, IRouteConfiguration, new()
    {
        // Default endpoint filters
        services.AddEndpointFilter<AuthorizationEndpointFilter>();

        services.AddHttpContextAccessor();
        services.AddScoped<IAuthenticatedUserService, DefaultAuthenticatedUserService>();

#if NET7_0
        services.AddSingleton<Dummy>();
#endif

        services.AddProblemDetails();

        var config = new ApiRouteConfiguration(services);

        config.AddRouteConfiguration<TRouteConfiguration>();

        configure?.Invoke(config);

        ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;

        foreach (var configurator in config._postConfigurators)
        {
            configurator.Invoke(config);
        }

        return services;
    }
}