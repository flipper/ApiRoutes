using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ApiRoutes.Swagger;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static ApiRouteConfiguration UseSwagger(this ApiRouteConfiguration configuration)
    {
        configuration.Services.AddEndpointFilter<SwaggerEndpointFilter>();
        configuration.Services.AddSingleton<ApiRouteOperationFilter>();
        configuration.Services.AddSingleton<IPostConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwagger>();

        return configuration;
    }

    public static IApplicationBuilder UseSwaggerUIAotFriendly(this IApplicationBuilder app, SwaggerUIOptions options)
    {
        return app.UseMiddleware<SwaggerUIMiddlewareAOTFriendly>(options);
    }

    /// <summary>
    /// Register the SwaggerUI middleware with optional setup action for DI-injected options. Supports AOT by using a JsonSerializerContext
    /// </summary>
    public static IApplicationBuilder UseSwaggerUIAotFriendly(
        this IApplicationBuilder app,
        Action<SwaggerUIOptions>? setupAction = null)
    {
        SwaggerUIOptions options;
        using (var scope = app.ApplicationServices.CreateScope())
        {
            options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SwaggerUIOptions>>().Value;
            setupAction?.Invoke(options);
        }

        // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
        if (options.ConfigObject.Urls == null)
        {
            var hostingEnv = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            options.ConfigObject.Urls = new[]
                { new UrlDescriptor { Name = $"{hostingEnv.ApplicationName} v1", Url = "v1/swagger.json" } };
        }

        return app.UseSwaggerUIAotFriendly(options);
    }
}