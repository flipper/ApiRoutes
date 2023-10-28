using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiRoutes.Swagger;

public class ConfigureSwagger : IPostConfigureOptions<SwaggerGeneratorOptions>
{
    private readonly ApiRouteOperationFilter _apiRouteOperationFilter;

    public ConfigureSwagger(ApiRouteOperationFilter apiRouteOperationFilter)
    {
        _apiRouteOperationFilter = apiRouteOperationFilter;
    }

    public void PostConfigure(string? name, SwaggerGeneratorOptions options)
    {
        options.OperationFilters.Add(_apiRouteOperationFilter);
    }
}