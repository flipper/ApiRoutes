namespace ApiRoutes.Generator;

public class IncrementalGeneratorResult : IGeneratorResult
{
    public List<ApiRouteData> ApiRoutes { get; } = new();
}