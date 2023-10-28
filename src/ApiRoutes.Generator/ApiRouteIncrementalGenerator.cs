using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Text;
using ApiRoutes.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ApiRoutes.Generator;

[Generator(LanguageNames.CSharp)]
public class ApiRouteIncrementalGenerator : IIncrementalGenerator
{
    private static readonly Dictionary<string, HttpStatusCode> _validRequestHandlerExtensionMethods = new()
    {
        { "Ok", HttpStatusCode.OK },
        { "OkAsTask", HttpStatusCode.OK },
        { "NoContent", HttpStatusCode.NoContent },
        { "NoContentAsTask", HttpStatusCode.NoContent },
        { "Accepted", HttpStatusCode.Accepted },
        { "AcceptedAsTask", HttpStatusCode.Accepted },

        { "Error", HttpStatusCode.InternalServerError }, // status code is not used
        { "ErrorAsTask", HttpStatusCode.InternalServerError } // status code is not used
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TypeDeclarationSyntax> typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax
                {
                    AttributeLists.Count: > 0
                }, // select types with attributes
                transform: static (ctx, _) =>
                {
                    var typeDeclarationSyntax = (TypeDeclarationSyntax)ctx.Node;

                    foreach (var attributeList in typeDeclarationSyntax.AttributeLists)
                    {
                        foreach (var attribute in attributeList.Attributes)
                        {
                            if (ModelExtensions.GetSymbolInfo(ctx.SemanticModel, attribute).Symbol is not IMethodSymbol
                                attributeSymbol)
                            {
                                // weird, we couldn't get the symbol, ignore it
                                continue;
                            }

                            var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                            var fullName = attributeContainingTypeSymbol.ToDisplayString();

                            // Is the attribute the [ApiRoute] attribute?
                            if (fullName == "ApiRoutes.ApiRouteAttribute")
                            {
                                // return the type
                                return typeDeclarationSyntax;
                            }
                        }
                    }

                    return null;
                })
            .Where(static m => m is not null)!;

        IncrementalValuesProvider<INamedTypeSymbol> handlers = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) =>
                    s is ClassDeclarationSyntax c && c.BaseList != null &&
                    c.BaseList.Types.Count > 0, // select types with attributes
                transform: static (ctx, cancellationToken) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, cancellationToken);

                    if (symbol is INamedTypeSymbol namedTypeSymbol && !namedTypeSymbol.IsAbstract)
                    {
                        foreach (var @interface in namedTypeSymbol.AllInterfaces)
                        {
                            if ((@interface.ConstructedFrom.FullName() == LibraryTypes.IHandlerWithoutResponse ||
                                 @interface.ConstructedFrom.FullName() == LibraryTypes.IHandlerWithResponse) &&
                                @interface.TypeArguments.Length > 0)
                            {
                                return namedTypeSymbol;
                            }
                        }
                    }

                    return null;
                })
            .Where(static m => m is not null)!;

        IncrementalValuesProvider<INamedTypeSymbol> validators = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) =>
                    s is ClassDeclarationSyntax c && c.BaseList != null &&
                    c.BaseList.Types.Count > 0, // select types with attributes
                transform: static (ctx, cancellationToken) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(ctx.Node, cancellationToken);

                    if (symbol is INamedTypeSymbol { IsAbstract: false, BaseType: not null } namedTypeSymbol)
                    {
                        if (namedTypeSymbol.BaseType.ConstructedFrom.FullName() == LibraryTypes.RequestValidator)
                        {
                            return namedTypeSymbol;
                        }
                    }

                    return null;
                })
            .Where(static m => m is not null)!;


        var compilationAndTypes
            = context.CompilationProvider.Combine(context.ParseOptionsProvider).Combine(typeDeclarations.Collect())
                .Combine(handlers.Collect()).Combine(validators.Collect());

        context.RegisterSourceOutput(compilationAndTypes,
            static (spc, source) => Execute(source.Left.Left.Left.Left, source.Left.Left.Left.Right,
                source.Left.Left.Right, source.Left.Right, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ParseOptions parseOptions,
        ImmutableArray<TypeDeclarationSyntax> requests,
        ImmutableArray<INamedTypeSymbol> handlers, ImmutableArray<INamedTypeSymbol> validators,
        SourceProductionContext spc)
    {
        Log.LogInfo($"Starting generation at: {DateTimeOffset.Now}");


        var result = new IncrementalGeneratorResult();
        var stopwatch = Stopwatch.StartNew();

        var isNet8 = parseOptions.PreprocessorSymbolNames.Any(s => s == "NET8_0");


        var generatorContext =
            new GeneratorContext(spc.AddSource, compilation.AssemblyName!, isNet8, result);

        Log.LogInfo($"Requests: {requests.Length}");
        Log.LogInfo($"Handlers: {handlers.Length}");
        Log.LogInfo($"Validators: {validators.Length}");

        foreach (var request in requests)
        {
            var symbol =
                (INamedTypeSymbol)ModelExtensions.GetDeclaredSymbol(compilation.GetSemanticModel(request.SyntaxTree),
                    request)!;

            var attribute = symbol.GetAttributes().SingleOrDefault(a => a.AttributeClass!.Name == "ApiRouteAttribute")!;

            var route = (string)attribute.ConstructorArguments[0].Value!;
            var httpMethod = (int)attribute.ConstructorArguments[1].Value!;

            var readBody = (bool?)attribute.NamedArguments.SingleOrDefault(a => a.Key == "ReadBody").Value
                .Value;
            var requireAuth =
                (bool?)attribute.NamedArguments.SingleOrDefault(a => a.Key == "RequireAuth").Value.Value;
            var authPolicy =
                (string?)attribute.NamedArguments.SingleOrDefault(a => a.Key == "AuthPolicy").Value.Value;

            var hasPrepareRequest = symbol.Interfaces.Any(i => i.Name == "IPrepareRequest");

            var apiRouteData = new ApiRouteData
            {
                Symbol = symbol,
                Route = route,
                Method = (LibraryTypes.HttpMethodEnum)httpMethod,
                HasPrepareMethod = hasPrepareRequest
            };

            if (requireAuth.HasValue && requireAuth.Value)
            {
                apiRouteData.AuthorizationData = new AuthorizationData
                {
                    Policy = authPolicy
                };
                Log.LogInfo("Policy: " + authPolicy);
            }

            var handler = handlers.Where(t => t.AllInterfaces.Any(i =>
                    i.Name == "IHandler" && i.TypeArguments.Length > 0 &&
                    i.TypeArguments[0].TryFullName()!.Equals(symbol.FullName(), StringComparison.Ordinal)))
                .Select(t =>
                {
                    var i = t.AllInterfaces.SingleOrDefault(
                        i => i.Name == "IHandler" && i.TypeArguments.Length == 2);

                    return i == null ? null : new Tuple<INamedTypeSymbol, INamedTypeSymbol>(t, i);
                }).SingleOrDefault();

            if (handler == null)
            {
                Log.LogWarning($"Could not find handler for request: {symbol.FullName()}");
                return;
            }

            Log.LogInfo("Handler found for: " + symbol.FullName());

            var invokeMethod = handler.Item1
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Method)
                .Cast<IMethodSymbol>()
                .Select(m =>
                    m.ExplicitInterfaceImplementations.Length > 0
                        ? (m.ExplicitInterfaceImplementations[0], m.DeclaringSyntaxReferences)
                        : (m, m.DeclaringSyntaxReferences))
                .FirstOrDefault(m => m.Item1.Name == "InvokeAsync");

            if (invokeMethod.Item1 == null)
            {
                Log.LogWarning($"Could not find InvokeAsync for handler: {handler.Item1.FullName()}");
                return;
            }

            Log.LogInfo("InvokeAsync found for: " + handler.Item1.FullName());

            foreach (var reference in invokeMethod.DeclaringSyntaxReferences)
            {
                var syntax = reference.GetSyntax();

                foreach (var node in syntax.ChildNodes().First(n => n.IsKind(SyntaxKind.Block))
                             .DescendantNodesAndSelf())
                {
                    if (node is InvocationExpressionSyntax invoke)
                    {
                        if (invoke.Expression is MemberAccessExpressionSyntax ex)
                        {
                            if (_validRequestHandlerExtensionMethods.TryGetValue(ex.Name.ToString(),
                                    out var extensionMethodStatusCode))
                            {
                                var arguments = invoke.ArgumentList.Arguments.ToList();

                                if (ex.Name.ToString().StartsWith("Error"))
                                {
                                    if (arguments.Count == 2)
                                    {
                                        var argument = arguments.First();
                                        if (argument.Expression is MemberAccessExpressionSyntax
                                                memberAccessExpressionSyntax &&
                                            Enum.TryParse<HttpStatusCode>(
                                                memberAccessExpressionSyntax.Name.ToString(),
                                                out var httpStatusCode))
                                        {
                                            if (!apiRouteData.Responses.ContainsKey(httpStatusCode))
                                            {
                                                apiRouteData.Responses.Add(httpStatusCode, null);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var nodes = arguments[0].ChildNodes().ToList();

                                        var firstNode = nodes.First();

                                        Tuple<INamedTypeSymbol, HttpStatusCode>?
                                            FindRequestExceptionInSyntaxNode(
                                                SyntaxNode n)
                                        {
                                            switch (n)
                                            {
                                                case InvocationExpressionSyntax invocationExpressionSyntax:
                                                    var correctSemanticModel = compilation.GetSemanticModel(
                                                            invocationExpressionSyntax.SyntaxTree);
                                                    
                                                    var newSymbol =
                                                        correctSemanticModel.GetSymbolInfo(
                                                            invocationExpressionSyntax);

                                                    if (newSymbol.Symbol != null)
                                                    {
                                                        return Utilities.FindRequestExceptionInSymbol(
                                                            compilation,
                                                            newSymbol.Symbol);
                                                    }

                                                    break;
                                                case ObjectCreationExpressionSyntax
                                                    objectCreationExpression:
                                                {
                                                    if (Utilities.TryGetException(compilation,
                                                            objectCreationExpression,
                                                            out var exception))
                                                    {
                                                        return exception;
                                                    }

                                                    break;
                                                }
                                            }

                                            return null;
                                        }

                                        var exception = FindRequestExceptionInSyntaxNode(firstNode);

                                        if (exception != null)
                                        {
                                            if (!apiRouteData.Responses.ContainsKey(exception.Item2))
                                            {
                                                apiRouteData.Responses.Add(exception.Item2, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Assume its a non error response
                                    if (!apiRouteData.Responses.ContainsKey(extensionMethodStatusCode))
                                    {
                                        apiRouteData.Responses.Add(extensionMethodStatusCode, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            apiRouteData.Handler = handler.Item1;
            apiRouteData.Response = (INamedTypeSymbol)handler.Item2.TypeArguments[1];

            Log.LogInfo("Looking for validator for: " + symbol.FullName());


            var validator =
                validators.FirstOrDefault(v => v.BaseType!.TypeArguments[0].TryFullName() == symbol.FullName());

            if (validator != null)
            {
                Log.LogInfo("Found validator for: " + symbol.FullName());
                apiRouteData.Validator = validator;
            }

            var properties = new Dictionary<(ISymbol, ITypeSymbol), ImmutableArray<AttributeData>>();

            if (symbol.IsRecord)
            {
                var baseConstructor =
                    symbol.Constructors.FirstOrDefault(c => !c.IsStatic && c.Parameters.Length > 0);

                if (baseConstructor != null)
                {
                    foreach (var parameter in baseConstructor.Parameters)
                    {
                        properties.Add((parameter, parameter.Type), parameter.GetAttributes());
                    }
                }
            }

            foreach (var property in symbol.WritableScalarProperties())
            {
                if (property == null) continue;

                properties.Add((property, property.Type), property.GetAttributes());
            }


            foreach (var pair in properties)
            {
                var apiRouteProperty = new ApiRouteProperty
                {
                    Symbol = pair.Key.Item1,
                    Type = pair.Key.Item2,
                    Method = ApiRoutePropertyFetchMethod.None,
                    Name = pair.Key.Item1.Name,
                    Documentation = XmlDocumentation.Parse(pair.Key.Item1.DeclaringSyntaxReferences
                        .First().GetSyntax())
                };

                foreach (var attributeData in pair.Value)
                {
                    var fetchMethod = attributeData.AttributeClass.FullName() switch
                    {
                        "global::Microsoft.AspNetCore.Mvc.FromRouteAttribute" => ApiRoutePropertyFetchMethod
                            .Route,
                        "global::Microsoft.AspNetCore.Mvc.FromQueryAttribute" => ApiRoutePropertyFetchMethod
                            .Query,
                        "global::Microsoft.AspNetCore.Mvc.FromHeaderAttribute" => ApiRoutePropertyFetchMethod
                            .Header,
                        "global::Microsoft.AspNetCore.Mvc.FromFormAttribute" => ApiRoutePropertyFetchMethod
                            .Form,
                        _ => ApiRoutePropertyFetchMethod.None
                    };

                    if (attributeData.AttributeClass.FullName() == "global::ApiRoutes.HideAttribute")
                    {
                        apiRouteProperty.IsHidden = true;
                    }

                    if (fetchMethod != ApiRoutePropertyFetchMethod.None)
                    {
                        var name = attributeData.NamedArguments.Any(a => a.Key == "Name")
                            ? (string)attributeData.NamedArguments.SingleOrDefault(a => a.Key == "Name")
                                .Value.Value!
                            : StringUtility.ToCamelCase(pair.Key.Item1.Name);

                        apiRouteProperty.Name = name;
                        apiRouteProperty.Method = fetchMethod;

                        Log.LogInfo($"Found {pair.Key.Item1} with attribute: " + attributeData);
                    }
                }

                apiRouteData.Properties.Add(apiRouteProperty);
            }

            apiRouteData.Documentation = XmlDocumentation.Parse(request);

            Log.LogInfo(apiRouteData.ToString());

            result.ApiRoutes.Add(apiRouteData);
        }


        ApiGeneratorWriter.CreateRoutesFile(generatorContext);
        ApiGeneratorWriter.CreateRouteConfigurationFile(generatorContext);
        ApiGeneratorWriter.CreateAuthenticationHelpers(generatorContext);
        ApiGeneratorWriter.CreateStringCacheFile(generatorContext);

        stopwatch.Stop();
        Log.LogInfo($"Generation took {stopwatch.ElapsedMilliseconds} ms");
    }
}