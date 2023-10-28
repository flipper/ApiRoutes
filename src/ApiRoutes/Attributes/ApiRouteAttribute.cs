using System.Diagnostics.CodeAnalysis;

namespace ApiRoutes;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class ApiRouteAttribute : Attribute
{
    [StringSyntax("Route")]
    public string Pattern { get; private set; }

    public Method HttpMethod { get; private set; }
    public bool RequireAuth { get; set; }
    public string? AuthPolicy { get; set; }

    public ApiRouteAttribute(
        [StringSyntax("Route")]
        string pattern,
        Method httpMethod)
    {
        Pattern = pattern;
        HttpMethod = httpMethod;
    }
}