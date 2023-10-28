using ApiRoutes.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiRoutes.Swagger;

public class ApiRouteOperationFilter : IOperationFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ApiRouteOperationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = $"/{context.ApiDescription.RelativePath}";
        var method = Enum.Parse<Method>(context.ApiDescription.HttpMethod!);

        var routes = _serviceProvider.GetServices<IRouteConfiguration>().SelectMany(c => c.Routes).ToDictionary(pair => pair.Key, pair => pair.Value);

        if (!routes.TryGetValue((path, method), out var routeMetadata))
        {
            return;
        }

        if (!string.IsNullOrEmpty(routeMetadata.Summary))
        {
            operation.Summary = routeMetadata.Summary;
        }
        
        if (!string.IsNullOrEmpty(routeMetadata.Description))
        {
            operation.Description = routeMetadata.Description;
        }

        operation.Parameters.Clear();
        
        foreach (var property in routeMetadata.Properties.Where(p => p.Method != PropertyFetchMethod.None && p.Method != PropertyFetchMethod.Form))
        {
            var location = property.Method switch
            {
                PropertyFetchMethod.Route => ParameterLocation.Path,
                PropertyFetchMethod.Query => ParameterLocation.Query,
                PropertyFetchMethod.Header => ParameterLocation.Header,
                _ => throw new ArgumentOutOfRangeException()
            };

            var item = new OpenApiParameter
            {
                Name = property.Name,
                In = location,
                Required = location == ParameterLocation.Path || property.Required
            };

            if (!string.IsNullOrEmpty(property.Description))
            {
                item.Description = property.Description;
            }
            
            operation.Parameters.Add(item);
        }

        context.SchemaGenerator.GenerateSchema(routeMetadata.Request, context.SchemaRepository);

        var reference = context.SchemaRepository.Schemas[routeMetadata.Request.Name];
        
        reference.Properties.Clear();
        

        foreach (var property in routeMetadata.Properties.Where(p => p is { IsHidden: false }))
        {
            if (routeMetadata.Type == RouteMetadata.RequestBodyType.Form && property.Method == PropertyFetchMethod.None)
            {
                continue;
            }
            
            var propertyReference = context.SchemaGenerator.GenerateSchema(property.Type, context.SchemaRepository);
            propertyReference.Description = property.Description;
            propertyReference.Nullable = property.Type.IsReferenceOrNullableType();

            reference.Properties.Add(StringUtility.ToCamelCase(property.Name), propertyReference);
        }


        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                {routeMetadata.Type == RouteMetadata.RequestBodyType.Form ? "multipart/form-data" : "application/json", new OpenApiMediaType { Schema = reference }}
            }
        };

        operation.Responses = new OpenApiResponses();
        
        context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);
        var problemDetailsSchemaReference = context.SchemaRepository.Schemas[nameof(ProblemDetails)];
        
        problemDetailsSchemaReference.Properties.Clear();

        problemDetailsSchemaReference.Properties.Add("type", new OpenApiSchema
        {
            Type = "string",
            Nullable = true
        });
            
        problemDetailsSchemaReference.Properties.Add("title", new OpenApiSchema
        {
            Type = "string",
            Nullable = true
        });
            
        problemDetailsSchemaReference.Properties.Add("status", new OpenApiSchema
        {
            Type = "integer",
            Format = "int32",
            Nullable = true
        });
            
        problemDetailsSchemaReference.Properties.Add("detail", new OpenApiSchema
        {
            Type = "string",
            Nullable = true
        });
            
        problemDetailsSchemaReference.Properties.Add("instance", new OpenApiSchema
        {
            Type = "string",
            Nullable = true
        });

        foreach (var pair in routeMetadata.Responses)
        {
            var schema = problemDetailsSchemaReference;
            
            if (pair.Key.IsSuccessStatusCode())
            {
                if (routeMetadata.Response == typeof(None))
                {
                    schema = null;
                }
                else
                {
                    schema = context.SchemaGenerator.GenerateSchema(routeMetadata.Response, context.SchemaRepository);
                }
            }
            
            operation.Responses.Add(((int)pair.Key).ToString(), new OpenApiResponse
            {
                Description = string.IsNullOrEmpty(pair.Value) ? ReasonPhrases.GetReasonPhrase((int)pair.Key) : pair.Value,
                Content = schema == null ? null : new Dictionary<string, OpenApiMediaType>
                {
                    { "application/json", new OpenApiMediaType { Schema = schema } }
                }
            });
        }

        context.SchemaRepository.Schemas.Remove("IResultTask");
    }
}