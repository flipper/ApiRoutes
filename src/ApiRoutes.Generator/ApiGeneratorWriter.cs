using System.Text;
using Microsoft.CodeAnalysis;

namespace ApiRoutes.Generator;

public static class ApiGeneratorWriter
{
    public static void CreateStringCacheFile(GeneratorContext context)
    {
        var code = new CodeWriter();

        code.AppendLine("#nullable enable");

        code.AppendLine("// Auto Generated");

        code.AppendLine("using System.Runtime.CompilerServices;");

        using (code.BeginScope($"namespace ApiRoutes.Generated.{context.AssemblyName}"))
        {
            code.AppendLine("[CompilerGenerated]");
            using (code.BeginScope("internal static class StringCache"))
            {
                foreach (var pair in context.StringCache)
                {
                    code.AppendLine($"public static readonly string {pair.Key} = \"{pair.Value}\";");
                }
            }
        }

        context.AddSource("StringCache.g.cs", code.ToString());
    }
    
    public static void CreateAuthenticationHelpers(GeneratorContext context)
    {
        foreach (var apiRoute in context.Result.ApiRoutes.Where(r => r.AuthorizationData != null))
        {
            var codeWriter = new CodeWriter();
            codeWriter.AppendLine("#nullable enable");

            codeWriter.AppendLine("// Auto Generated");

            codeWriter.AppendLine("using System.Runtime.CompilerServices;");

            using (codeWriter.BeginScope($"namespace {apiRoute.Symbol.FullNamespace(false)}"))
            {
                var modifiers = new List<string>();

                if (apiRoute.Symbol.IsRecord)
                {
                    modifiers.Add("record");
                    if (apiRoute.Symbol.IsValueType)
                    {
                        modifiers.Add("struct");
                    }
                }
                else
                {
                    if (apiRoute.Symbol.IsValueType)
                    {
                        modifiers.Add("struct");
                    }
                    else
                    {
                        modifiers.Add("class");
                    }
                }

                codeWriter.AppendLine("[CompilerGenerated]");
                using (codeWriter.BeginScope($"public partial {string.Join(" ", modifiers)} {apiRoute.Symbol.Name}"))
                {
                    codeWriter.AppendLine("public string AuthenticatedUserId { get; set; }");
                }
            }

            context.AddSource($"{apiRoute.Symbol.FullNamespace(false)}.{apiRoute.Symbol.Name}.g.cs", codeWriter.ToString());
        }
    }

    public static void CreateRouteConfigurationFile(GeneratorContext context)
    {
        string DefaultToEmptyStringIfEmpty(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "string.Empty";
            }

            return $"\"{value}\"";
        }
        
        var code = new CodeWriter();

        code.AppendLine("#nullable enable");

        code.AppendLine("// Auto Generated");
        
        code.AppendLine("using System.Net;");
        code.AppendLine("using Microsoft.AspNetCore.Routing;");
        code.AppendLine("using Microsoft.AspNetCore.Mvc;");
        code.AppendLine("using Microsoft.AspNetCore.Http.HttpResults;");
        code.AppendLine("using Microsoft.AspNetCore.Http;");
        code.AppendLine("using FluentValidation;");
        code.AppendLine("using System.Text.Json;");
        code.AppendLine("using System.Runtime.CompilerServices;");
        code.AppendLine("using ApiRoutes;");
        code.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        code.AppendLine("using Microsoft.AspNetCore.Builder;");
        code.AppendLine("using Microsoft.Extensions.Primitives;");
        code.AppendLine("using System.Globalization;");

        if (context.IsNet8)
        {
            code.AppendLine("using System.Collections.ObjectModel;");
        }
        
        using (code.BeginScope($"namespace ApiRoutes.Generated.{context.AssemblyName}"))
        {
            code.AppendLine("[CompilerGenerated]");
            using (code.BeginScope($"public sealed class {context.AssemblyName!.Replace(".", string.Empty)}GeneratedRouteConfiguration : IRouteConfiguration"))
            {
                using (code.BeginScope($"public Dictionary<(string, {LibraryTypes.HttpMethod}), RouteMetadata> Routes {{ get; }} = new()",
                           new CodeWriter.Settings
                           {
                               StartBracket = "{",
                               EndBracket = "};"
                           }))
                {
                    foreach (var apiRoute in context.Result.ApiRoutes)
                    {
                        var templateCacheKey = context.StringCache.Add(apiRoute.Symbol.FullName()!, apiRoute.Route);

                        using (code.BeginScope(
                                   $"{{ ({templateCacheKey}, {LibraryTypes.HttpMethod}.{apiRoute.Method}), new RouteMetadata {{",
                                   new CodeWriter.Settings
                                   {
                                       EndBracket = "}},"
                                   }))
                        {
                            code.AppendLine($"Template = {templateCacheKey},");
                            code.AppendLine($"Method = {LibraryTypes.HttpMethod}.{apiRoute.Method},");
                            code.AppendLine($"Summary = {DefaultToEmptyStringIfEmpty(apiRoute.Documentation.Summary)},");
                            code.AppendLine($"Description = {DefaultToEmptyStringIfEmpty(apiRoute.Documentation.Remarks)},");
                            code.AppendLine($"Type = {LibraryTypes.RequestBodyType}.{(apiRoute.ReadJsonBody ? "Json" : apiRoute.ReadForm ? "Form" : "None")},");
                            code.AppendLine($"Request = typeof({apiRoute.Symbol.FullName()}),");
                            code.AppendLine($"Response = typeof({apiRoute.Response.FullName()}),");
                            code.AppendLine($"Handler = typeof({apiRoute.Handler.FullName()}),");
                            var s = apiRoute.Validator == null ? "null" : $"typeof({apiRoute.Validator.FullName()})";
                            code.AppendLine($"Validator = {s},");
                            code.AppendLine($"RequiresAuth = {(apiRoute.AuthorizationData != null ? "true" : "false")},");

                            var policy = apiRoute.AuthorizationData != null
                                ? DefaultToEmptyStringIfEmpty(apiRoute.AuthorizationData.Policy)
                                : DefaultToEmptyStringIfEmpty(string.Empty);
                            
                            code.AppendLine($"AuthPolicy = {policy},");

                            var propertiesStringBuilder = new StringBuilder();

                            foreach (var property in apiRoute.Properties)
                            {
                                var cacheKey = context.StringCache.Add($"{apiRoute.Symbol.FullName()}_{property.Name}",
                                    property.Name);


                                propertiesStringBuilder.Append(
                                    $"new ({cacheKey}, typeof({property.Type.TryFullName()!.Replace("?", string.Empty)}), {(property.Type.NullableAnnotation == NullableAnnotation.NotAnnotated).ToString().ToLower()}, PropertyFetchMethod.{property.Method}, {DefaultToEmptyStringIfEmpty(property.Documentation.Summary)}, {property.IsHidden.ToString().ToLower()}), ");
                            }

                            code.AppendLine($$"""
Properties = new () { {{propertiesStringBuilder}} },
""");

                            var responsesStringBuilder = new StringBuilder();

                            foreach (var response in apiRoute.Responses)
                            {
                                var value = response.Value == null ? "null" : $"\"{response.Value}\"";
                                responsesStringBuilder.Append($$"""{ HttpStatusCode.{{response.Key}}, {{value}} }, """);
                            }

                            code.AppendLine($$"""
Responses = new () { {{responsesStringBuilder}} },
""");
                        }
                    }
                }


                if (context.IsNet8)
                {
                    AppendAotHelpers(context, code);
                }
                
                code.AppendLine();
                AppendRouteHandlerMethods(context, code);
                code.AppendLine();
                AppendMapRoutes(context, code);
                code.AppendLine();

                using (code.BeginScope("public void RegisterServices(IServiceCollection services)"))
                {
                    // code.AppendLine($"services.PostConfigure<global::Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.AddContext<global::ApiRoutes.Generated.{context.Compilation.AssemblyName}.ApiRoutesJsonSerializerContext>());");
                    
                    foreach (var apiRoute in context.Result.ApiRoutes)
                    {
                        code.AppendLine($"services.AddScoped<IRequestHandler<{apiRoute.Symbol.FullName()}, RequestResult>, RequestHandler<{apiRoute.Symbol.FullName()}, RequestResult>>();");
                        code.AppendLine($"services.AddScoped<IRequestHandlerCore<{apiRoute.Symbol.FullName()}, RequestResult>, {apiRoute.Handler.FullName()}>();");

                        if (apiRoute.Validator != null)
                        {
                            code.AppendLine($"services.AddScoped<IValidator<{apiRoute.Symbol.FullName()}>, {apiRoute.Validator.FullName()}>();");
                        }
                    }
                }
            }
        }

        context.AddSource("GeneratedRouteConfiguration.g.cs", code.ToString());
    }

    private static void AppendAotHelpers(GeneratorContext context, CodeWriter code)
    {
        foreach (var apiRoute in context.Result.ApiRoutes)
        {
            using (code.BeginScope(
                       $"private static Microsoft.AspNetCore.Http.RequestDelegateMetadataResult {apiRoute.Symbol.SafeFullName()}_metadata(System.Reflection.MethodInfo methodInfo, Microsoft.AspNetCore.Http.RequestDelegateFactoryOptions? options)"))
            {
                code.AppendLine(
                    "return new Microsoft.AspNetCore.Http.RequestDelegateMetadataResult { EndpointMetadata = options.EndpointBuilder.Metadata.AsReadOnly() };");
            }

            code.AppendLine();

            using (code.BeginScope(
                       $"private static Microsoft.AspNetCore.Http.RequestDelegateResult {apiRoute.Symbol.SafeFullName()}_createRequestDelegate(System.Delegate del, Microsoft.AspNetCore.Http.RequestDelegateFactoryOptions options, Microsoft.AspNetCore.Http.RequestDelegateMetadataResult? inferredMetadataResult)"))
            {

                if (apiRoute.ReadJsonBody)
                {
                    code.AppendLine(
                        $"var handler = (Func<global::Microsoft.AspNetCore.Http.HttpContext, {apiRoute.Symbol.FullName()}, System.Threading.Tasks.Task<global::Microsoft.AspNetCore.Http.IResult>>)del;");
                }
                else
                {
                    code.AppendLine(
                        "var handler = (Func<global::Microsoft.AspNetCore.Http.HttpContext, System.Threading.Tasks.Task<global::Microsoft.AspNetCore.Http.IResult>>)del;");
                }
                
                
                code.AppendLine("EndpointFilterDelegate? filteredInvocation = null;");
                using (code.BeginScope("if (options?.EndpointBuilder?.FilterFactories.Count > 0)"))
                {
                    if (apiRoute.ReadJsonBody)
                    {
                        code.AppendLine($$"""
filteredInvocation = RouteBuilderUtility.BuildFilterDelegate(ic =>
                        {
                            if (ic.HttpContext.Response.StatusCode == 400)
                            {
                                return ValueTask.FromResult<object?>(Results.Empty);
                            }
                            return ValueTask.FromResult<object?>(handler(ic.GetArgument<global::Microsoft.AspNetCore.Http.HttpContext>(0)! , ic.GetArgument<{{apiRoute.Symbol.FullName()}}>(1)!));
                        },
                        options.EndpointBuilder,
                        handler.Method);
""");
                    }
                    else
                    {
                        code.AppendLine("""
filteredInvocation = RouteBuilderUtility.BuildFilterDelegate(ic =>
                        {
                            if (ic.HttpContext.Response.StatusCode == 400)
                            {
                                return ValueTask.FromResult<object?>(Results.Empty);
                            }
                            return ValueTask.FromResult<object?>(handler(ic.GetArgument<global::Microsoft.AspNetCore.Http.HttpContext>(0)!));
                        },
                        options.EndpointBuilder,
                        handler.Method);
""");
                    }
                }


                using (code.BeginScope("async Task RequestHandler(HttpContext httpContext)"))
                {
                    code.AppendLine("var context_local = httpContext;");
                    
                    
                    if (apiRoute.ReadJsonBody)
                    {
                        code.AppendLine($"var request_resolveBodyResult = await RouteBuilderUtility.TryResolveBodyAsync<{apiRoute.Symbol.FullName()}>(httpContext, false);");
                        code.AppendLine("var request_local = request_resolveBodyResult.Item2;");
                        using (code.BeginScope("if (!request_resolveBodyResult.Item1)"))
                        {
                            code.AppendLine("return;");
                        }
                    }


                    if (apiRoute.ReadJsonBody)
                    {
                        code.AppendLine("var result = await handler(context_local, request_local!);");
                    }
                    else
                    {
                        code.AppendLine("var result = await handler(context_local);");
                    }
                    
                    code.AppendLine("await result.ExecuteAsync(httpContext);");
                }

                using (code.BeginScope("async Task RequestHandlerFiltered(HttpContext httpContext)"))
                {
                    code.AppendLine("var context_local = httpContext;");


                    if (apiRoute.ReadJsonBody)
                    {
                        code.AppendLine($"var request_resolveBodyResult = await RouteBuilderUtility.TryResolveBodyAsync<{apiRoute.Symbol.FullName()}>(httpContext, false);");
                        code.AppendLine("var request_local = request_resolveBodyResult.Item2;");
                        using (code.BeginScope("if (!request_resolveBodyResult.Item1)"))
                        {
                            code.AppendLine("return;");
                        }
                    }

                    if (apiRoute.ReadJsonBody)
                    {
                        code.AppendLine(
                            $"var result = await filteredInvocation(EndpointFilterInvocationContext.Create<global::Microsoft.AspNetCore.Http.HttpContext, {apiRoute.Symbol.FullName()}>(httpContext, context_local, request_local!));");
                    }
                    else
                    {
                        code.AppendLine(
                            "var result = await filteredInvocation(EndpointFilterInvocationContext.Create<global::Microsoft.AspNetCore.Http.HttpContext>(httpContext, context_local));");
                    }
                    
                    code.AppendLine("await RouteBuilderUtility.ExecuteObjectResult(result, httpContext);");
                }

                code.AppendLine(
                    "RequestDelegate targetDelegate = filteredInvocation is null ? RequestHandler : RequestHandlerFiltered;");
                code.AppendLine(
                    "var metadata = inferredMetadataResult?.EndpointMetadata ?? ReadOnlyCollection<object>.Empty;");

                code.AppendLine("return new RequestDelegateResult(targetDelegate, metadata);");
            }
        }
    }

    private static void AppendMapRoutes(GeneratorContext context, CodeWriter code)
    {
        using (code.BeginScope("public void MapRoutes(IEndpointRouteBuilder app)"))
        {
            code.AppendLine(
                $"var configuration = app.ServiceProvider.GetRequiredService<{context.AssemblyName!.Replace(".", string.Empty)}GeneratedRouteConfiguration>();");

            code.AppendLine("var filters = app.ServiceProvider.GetServices<IEndpointFilter>();");


            foreach (var apiRoute in context.Result.ApiRoutes)
            {
                var name = apiRoute.Symbol.Name.Replace("Query", string.Empty)
                    .Replace("Command", string.Empty);


                string lambda;
                
                if (context.IsNet8)
                {
                    lambda = $$"""
var {{name}}Builder = RouteHandlerServices.Map(app, global::ApiRoutes.Generated.{{context.AssemblyName}}.Routes.{{name}}, __{{apiRoute.Handler.SafeFullName()}}, new[] { "{{apiRoute.Method}}" }, {{apiRoute.Symbol.SafeFullName()}}_metadata, {{apiRoute.Symbol.SafeFullName()}}_createRequestDelegate);
""";
                }
                else
                {
                    lambda = $$"""
var {{name}}Builder = app.MapMethods(global::ApiRoutes.Generated.{{context.AssemblyName}}.Routes.{{name}}, new[] { "{{apiRoute.Method}}" }, __{{apiRoute.Handler.SafeFullName()}});
""";
                }
                
                code.AppendLine(lambda);
            }

            using (code.BeginScope("foreach(var filter in filters)"))
            {
                for (var index = 0; index < context.Result.ApiRoutes.Count; index++)
                {
                    var apiRoute = context.Result.ApiRoutes[index];

                    var name = apiRoute.Symbol.Name.Replace("Query", string.Empty)
                        .Replace("Command", string.Empty);

                    code.AppendLine(
                        $"filter.Handle(configuration.Routes[(global::ApiRoutes.Generated.{context.AssemblyName}.Routes.{name}, {LibraryTypes.HttpMethod}.{apiRoute.Method})], {name}Builder);");
                }
            }
        }
    }
    
    private static void AppendRouteHandlerMethods(GeneratorContext context, CodeWriter code)
    {
        foreach (var apiRoute in context.Result.ApiRoutes)
        {
            var arguments = new List<string>
            {
                "HttpContext context"
            };

            if (!context.IsNet8)
            {
                arguments.Add("Dummy _");
            }

            if (apiRoute.ReadJsonBody)
            {
                arguments.Add($"[FromBody] {apiRoute.Symbol.FullName()} request");
            }

            var lambda = $$"""
private static async Task<IResult> __{{apiRoute.Handler.SafeFullName()}}({{string.Join(", ", arguments)}})
""";

            using (code.BeginScope(lambda))
            {
                var dict = new Dictionary<ApiRouteProperty, string>();
                
                if (apiRoute.ReadForm)
                {
                    code.AppendLine();
                    code.AppendLine("var form = await context.Request.ReadFormAsync(context.RequestAborted);");
                    code.AppendLine();
                }

                foreach (var property in apiRoute.Properties.Where(p => p.Method != ApiRoutePropertyFetchMethod.None && !p.Type.TryFullName().Contains("IFormFile")))
                {
                    var cacheKey = context.StringCache.Add($"{apiRoute.Symbol.FullName()}_{property.Name}",
                        property.Name);

                    var typeName = property.Type.TryFullName();

                    var isOptional = property.Type.NullableAnnotation == NullableAnnotation.Annotated;
                    
                    //IArrayTypeSymbol

                    
                    var isArray = typeName!.Contains("[]");
                    

                    if (property.Method == ApiRoutePropertyFetchMethod.Query)
                    {
                        code.AppendLine($"var {property.Symbol.Name}_raw = context.Request.Query[{cacheKey}];");
                    } else if (property.Method == ApiRoutePropertyFetchMethod.Header)
                    {
                        code.AppendLine($"var {property.Symbol.Name}_raw = context.Request.Headers[{cacheKey}];");
                    } else if (property.Method == ApiRoutePropertyFetchMethod.Route)
                    {
                        code.AppendLine($"var {property.Symbol.Name}_raw = (string) context.Request.RouteValues[{cacheKey}];");
                    } else if (property.Method == ApiRoutePropertyFetchMethod.Form)
                    {
                        code.AppendLine($"var {property.Symbol.Name}_raw = form[{cacheKey}];");
                    }

                    if (!isOptional && property.Method != ApiRoutePropertyFetchMethod.Route)
                    {
                        using (code.BeginScope($"if(StringValues.IsNullOrEmpty({property.Symbol.Name}_raw))"))
                        {
                            code.AppendLine($"Console.WriteLine(\"No value for {property.Name}\");");
                            code.AppendLine("return TypedResults.BadRequest();");
                        }
                    }
                    
                    if (isArray)
                    {
                        //code.AppendLine($"request.{property.Symbol.Name} = {property.Symbol.Name}_raw.ToArray();");
                        dict.Add(property, $"{property.Symbol.Name}_raw.ToArray()");
                    }
                    else
                    {
                        // parse to correct type

                        if (typeName!.StartsWith("global::System.String"))
                        {
                            if (isOptional)
                            {
                                if (property.Method == ApiRoutePropertyFetchMethod.Route)
                                {
                                    dict.Add(property, $"({typeName}){property.Symbol.Name}_raw");
                                }
                                else
                                {
                                    dict.Add(property, $"{property.Symbol.Name}_raw.Count > 0 ? ({typeName}){property.Symbol.Name}_raw : null");
                                }
                            }
                            else
                            {
                                dict.Add(property, $"({typeName}){property.Symbol.Name}_raw");
                            }
                        }
                        else
                        {
                            if (property.Method == ApiRoutePropertyFetchMethod.Route)
                            {
                                code.AppendLine($"var {property.Symbol.Name}_temp = (string){property.Symbol.Name}_raw;");
                            }
                            else
                            {
                                code.AppendLine($"var {property.Symbol.Name}_temp = {property.Symbol.Name}_raw.Count > 0 ? (string?){property.Symbol.Name}_raw : null;");
                            }

                            var nonNullableTypeName = property.Type.Name;

                            if (property.Type.NullableAnnotation == NullableAnnotation.Annotated)
                            {
                                if (property.Type is INamedTypeSymbol symbolType)
                                {
                                    nonNullableTypeName = symbolType.ToString().Replace("?", string.Empty);
                                }
                            }


                            var isBoolean = nonNullableTypeName == "Boolean" || nonNullableTypeName == "bool";

                            if (isOptional)
                            {
                                code.AppendLine($"{typeName} {property.Symbol.Name}_parsed = default;");

                                if (isBoolean)
                                {
                                    using (code.BeginScope($"if({nonNullableTypeName}.TryParse({property.Symbol.Name}_temp, out var {property.Symbol.Name}_temp_parsed))"))
                                    {
                                        code.AppendLine($"{property.Symbol.Name}_parsed = {property.Symbol.Name}_temp_parsed;");
                                    }
                                }
                                else
                                {
                                    using (code.BeginScope($"if({nonNullableTypeName}.TryParse({property.Symbol.Name}_temp, CultureInfo.InvariantCulture, out var {property.Symbol.Name}_temp_parsed))"))
                                    {
                                        code.AppendLine($"{property.Symbol.Name}_parsed = {property.Symbol.Name}_temp_parsed;");
                                    }
                                }

                                using (code.BeginScope($"else if(string.IsNullOrEmpty({property.Symbol.Name}_temp))"))
                                {
                                    code.AppendLine($"{property.Symbol.Name}_parsed = null;");
                                }

                                using (code.BeginScope("else"))
                                {
                                    code.AppendLine($"Console.WriteLine(\"Failed to parse value for {property.Name}\");");
                                    code.AppendLine("return TypedResults.BadRequest();");
                                }
                                
                                dict.Add(property, $"{property.Symbol.Name}_parsed");
                            }
                            else
                            {
                                if (isBoolean)
                                {
                                    using (code.BeginScope($"if(!{nonNullableTypeName}.TryParse({property.Symbol.Name}_temp, out var {property.Symbol.Name}_temp_parsed))"))
                                    {
                                        code.AppendLine($"Console.WriteLine(\"Failed to parse value for {property.Name}. Required!\");");
                                        code.AppendLine("return TypedResults.BadRequest();");
                                    }
                                }
                                else
                                {
                                    using (code.BeginScope($"if(!{nonNullableTypeName}.TryParse({property.Symbol.Name}_temp, CultureInfo.InvariantCulture, out var {property.Symbol.Name}_temp_parsed))"))
                                    {
                                        code.AppendLine($"Console.WriteLine(\"Failed to parse value for {property.Name}. Required!\");");
                                        code.AppendLine("return TypedResults.BadRequest();");
                                    }
                                }
                                
                                dict.Add(property, $"{property.Symbol.Name}_temp_parsed");
                            }
                        }
                    }
                }

                
                // Support IFormFile mappings
                foreach (var property in apiRoute.Properties.Where(p => p.Method == ApiRoutePropertyFetchMethod.Form && p.Type.TryFullName().Contains("IFormFile")))
                {
                    var cacheKey = context.StringCache.Add($"{apiRoute.Symbol.FullName()}_{property.Name}",
                        property.Name);

                    var typeName = property.Type.TryFullName()!;
                    
                    var isOptional = property.Type.NullableAnnotation == NullableAnnotation.Annotated;

                    if (typeName.Contains("IReadOnlyList"))
                    {
                        dict.Add(property, $"form.Files.GetFiles({cacheKey})");
                    }
                    else
                    {
                        if (isOptional)
                        {
                            dict.Add(property, $"form.Files.GetFile({cacheKey})");
                        }
                        else
                        {
                            code.AppendLine($"var {property.Symbol.Name}_raw = form.Files.GetFile({cacheKey});");

                            using (code.BeginScope($"if({property.Symbol.Name}_raw == null)"))
                            {
                                code.AppendLine($"Console.WriteLine(\"No value for {property.Name}\");");
                                code.AppendLine("return TypedResults.BadRequest();");
                            }
                            
                            dict.Add(property, $"{property.Symbol.Name}_raw");
                        }
                    }
                }
                
                
                if (apiRoute.Symbol.IsRecord)
                {
                    if (!apiRoute.ReadJsonBody)
                    {
                        var constructorArgs = dict.Where(p => p.Key.Symbol is IParameterSymbol).Select(d => d.Value);
                        
                        code.AppendLine($"var request = new {apiRoute.Symbol.FullName()}({string.Join(", ", constructorArgs)});");
                        
                        foreach (var pair in dict.Where(p => p.Key.Symbol is IPropertySymbol))
                        {
                            code.AppendLine($"request.{pair.Key.Symbol.Name} = {pair.Value};");
                        }
                    }
                    else
                    {
                        foreach (var pair in dict)
                        {
                            code.AppendLine($"request.{pair.Key.Symbol.Name} = {pair.Value};");
                        }
                    }
                }
                else
                {
                    if (!apiRoute.ReadJsonBody)
                    {
                        code.AppendLine($"var request = new {apiRoute.Symbol.FullName()}();");
                    }

                    foreach (var pair in dict)
                    {
                        code.AppendLine($"request.{pair.Key.Symbol.Name} = {pair.Value};");
                    }
                }
                
                if (apiRoute.HasPrepareMethod)
                {
                    code.AppendLine();
                    code.AppendLine("request = await request.PrepareAsync(context, context.RequestAborted);");
                    code.AppendLine();
                }

                if (apiRoute.AuthorizationData != null)
                {
                    code.AppendLine("request.AuthenticatedUserId = context.RequestServices.GetRequiredService<global::ApiRoutes.Authentication.IAuthenticatedUserService>().UserId;");
                }
                

                if (apiRoute.Validator != null)
                {
                    code.AppendLine($"var validator = context.RequestServices.GetRequiredService<IValidator<{apiRoute.Symbol.FullName()}>>();");
                    code.AppendLine(
                        "var validationResult = await validator.ValidateAsync(request, context.RequestAborted);");

                    code.AppendLine();

                    using (code.BeginScope("if(!validationResult.IsValid)"))
                    {
                        code.AppendLine(
                            "return TypedResults.ValidationProblem(validationResult.ToDictionary());");
                    }
                }

                code.AppendLine($"var handler = context.RequestServices.GetRequiredService<IRequestHandler<{apiRoute.Symbol.FullName()}, RequestResult>>();");

                code.AppendLine(
                    "var response = await handler.InvokeAsync(request, context.RequestAborted);");

                code.AppendLine();

                using (code.BeginScope("if (response.IsError)"))
                {
                    code.AppendLine(
                        "return TypedResults.Problem(new ProblemDetails { Status = (int)response.Error.StatusCode, Detail = response.Error.Message });");
                }

                code.AppendLine("return response.Result;");
            }
        }
    }
    
    public static void CreateRoutesFile(GeneratorContext context)
    {
        var code = new CodeWriter();

        code.AppendLine("#nullable enable");

        code.AppendLine("// Auto Generated");

        code.AppendLine("using Microsoft.AspNetCore.Routing;");
        code.AppendLine("using Microsoft.AspNetCore.Mvc;");
        code.AppendLine("using Microsoft.AspNetCore.Http.HttpResults;");
        code.AppendLine("using Microsoft.AspNetCore.Http;");
        code.AppendLine("using FluentValidation;");
        code.AppendLine("using System.Text.Json;");
        code.AppendLine("using System.Runtime.CompilerServices;");
        code.AppendLine("using ApiRoutes;");
        code.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        code.AppendLine("using Microsoft.AspNetCore.Builder;");

        using (code.BeginScope($"namespace ApiRoutes.Generated.{context.AssemblyName}"))
        {
            code.AppendLine("[CompilerGenerated]");
            using (code.BeginScope("public static class Routes"))
            {
                foreach (var apiRoute in context.Result.ApiRoutes)
                {
                    var name = apiRoute.Symbol.Name.Replace("Query", string.Empty).Replace("Command", string.Empty);

                    code.AppendLine("/// <summary>");
                    code.AppendLine($"/// <see cref=\"{apiRoute.Symbol.FullName()}\"/>");
                    code.AppendLine("/// </summary>");

                    var cacheKey = context.StringCache.Add(apiRoute.Symbol.FullName()!, apiRoute.Route);

                    code.AppendLine($"""
public static readonly string {name} = {cacheKey};
""");
                }
            }
        }

        code.AppendLine("// END OF FILE");

        context.AddSource("Routes.g.cs", code.ToString());
    }
}