using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace ApiRoutes.EFCore;

public static class Extensions
{
    [RequiresDynamicCode("Uses MakeGenericType")]
    public static ApiRouteConfiguration UseEFCore(this ApiRouteConfiguration configuration)
    {
        var databaseBehaviourConfiguration = new DatabaseBehaviourConfiguration();
        
        foreach (var (_, metadata) in configuration.RouteConfigurations.SelectMany(c => c.Routes))
        {
            if (metadata.Handler.BaseType is { IsGenericType: true })
            {
                Type? dbContext = null;

                if (metadata.Handler.BaseType.GetGenericTypeDefinition() == typeof(HandlerWithDatabase<,>))
                {
                    dbContext = metadata.Handler.BaseType.GetGenericArguments()[1];
                }
                else if (metadata.Handler.BaseType.GetGenericTypeDefinition() == typeof(HandlerWithDatabase<,,>))
                {
                    dbContext = metadata.Handler.BaseType.GetGenericArguments()[2];
                }

                if (dbContext == null) continue;

                var @interface = typeof(IHandlerFilter<,>).MakeGenericType(metadata.Request, metadata.Response);
                var implementation = typeof(DatabaseBehaviour<,>).MakeGenericType(metadata.Request, metadata.Response);

                databaseBehaviourConfiguration.Configurations.Add(metadata.Request, dbContext);

                configuration.Services.AddScoped(@interface, implementation);
            }
        }

        configuration.Services.AddSingleton(databaseBehaviourConfiguration);

        return configuration;
    }
}