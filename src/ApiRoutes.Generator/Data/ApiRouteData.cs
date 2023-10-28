using System.Net;
using Microsoft.CodeAnalysis;

namespace ApiRoutes.Generator;

public class ApiRouteData
{
    public INamedTypeSymbol Symbol { get; set; } = null!;
    
    public XmlDocumentation Documentation { get; set; } = null!;

    public string Route { get; set; } = null!;
    public LibraryTypes.HttpMethodEnum Method { get; set; }

    public bool ReadForm => Properties.Any(p => p.Method == ApiRoutePropertyFetchMethod.Form);

    public bool HasPrepareMethod { get; set; }

    public INamedTypeSymbol? Validator { get; set; }
    
    public List<ApiRouteProperty> Properties { get; set; } = new();

    public INamedTypeSymbol Handler { get; set; } = null!;
    public INamedTypeSymbol Response { get; set; } = null!;

    public AuthorizationData? AuthorizationData { get; set; }

    public Dictionary<HttpStatusCode, string?> Responses { get; set; } = new();

    public bool ReadJsonBody => !ReadForm && (Method == LibraryTypes.HttpMethodEnum.POST ||
                                              Method == LibraryTypes.HttpMethodEnum.PUT ||
                                              Method == LibraryTypes.HttpMethodEnum.DELETE ||
                                              Method == LibraryTypes.HttpMethodEnum.PATCH);


    public override string ToString()
    {
        return $"{nameof(Symbol)}: {Symbol}, {nameof(Documentation)}: {Documentation}, {nameof(Route)}: {Route}, {nameof(Method)}: {Method}, {nameof(HasPrepareMethod)}: {HasPrepareMethod}, {nameof(Validator)}: {Validator}, {nameof(Properties)}: {Properties}, {nameof(Handler)}: {Handler}, {nameof(Response)}: {Response}, {nameof(AuthorizationData)}: {AuthorizationData}, {nameof(Responses)}: {Responses}";
    }
}

public class AuthorizationData
{
    public string? Policy { get; set; }
}

public enum ApiRoutePropertyFetchMethod
{
    None,
    Route,
    Form,
    Query,
    Header
}

public class ApiRouteProperty
{
    public ISymbol Symbol { get; set; } = null!;

    public ITypeSymbol Type { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ApiRoutePropertyFetchMethod Method { get; set; }

    public XmlDocumentation Documentation { get; set; } = null!;

    public bool IsHidden { get; set; }
}