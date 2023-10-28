#if NET7_0
namespace ApiRoutes;

/// <summary>
/// This is injected when using .NET 7 to fix a bug with ASP.NET Core. Where RequestDelegate does not work when there is only a HttpContext argument.
/// </summary>
public sealed class Dummy
{
    
}
#endif