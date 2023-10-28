#if NET8_0_OR_GREATER
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace ApiRoutes;

public static class RouteBuilderUtility
{
    public static Task ExecuteObjectResult(object? obj, HttpContext httpContext)
    {
        if (obj is IResult r)
        {
            return r.ExecuteAsync(httpContext);
        }

        if (obj is string s)
        {
            return httpContext.Response.WriteAsync(s);
        }

        return httpContext.Response.WriteAsJsonAsync(obj);
    }

    public static async ValueTask<(bool, T?)> TryResolveBodyAsync<T>(HttpContext httpContext, bool allowEmpty)
    {
        var feature = httpContext.Features.Get<IHttpRequestBodyDetectionFeature>();

        if (feature?.CanHaveBody == true)
        {
            if (!httpContext.Request.HasJsonContentType())
            {
                httpContext.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return (false, default);
            }

            try
            {
                var bodyValue = await httpContext.Request.ReadFromJsonAsync<T>();
                if (!allowEmpty && bodyValue == null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return (false, bodyValue);
                }

                return (true, bodyValue);
            }
            catch (IOException)
            {
                return (false, default);
            }
            catch (JsonException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return (false, default);
            }
        }

        if (!allowEmpty)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        return (false, default);
    }

    public static EndpointFilterDelegate BuildFilterDelegate(EndpointFilterDelegate filteredInvocation,
        EndpointBuilder builder, MethodInfo mi)
    {
        var routeHandlerFilters = builder.FilterFactories;
        var context0 = new EndpointFilterFactoryContext
        {
            MethodInfo = mi,
            ApplicationServices = builder.ApplicationServices,
        };
        for (var i = routeHandlerFilters.Count - 1; i >= 0; i--)
        {
            var filterFactory = routeHandlerFilters[i];
            filteredInvocation = filterFactory(context0, filteredInvocation);
        }

        return filteredInvocation;
    }

    public static Func<HttpContext, bool, ValueTask<(bool, T?)>> ResolveJsonBodyOrService<T>(
        IServiceProviderIsService? serviceProviderIsService = null)
    {
        if (serviceProviderIsService is not null)
        {
            if (serviceProviderIsService.IsService(typeof(T)))
            {
                return static (httpContext, isOptional) =>
                    new ValueTask<(bool, T?)>((true, httpContext.RequestServices.GetService<T>()));
            }
        }

        return static (httpContext, isOptional) => TryResolveBodyAsync<T>(httpContext, isOptional);
    }
}
#endif