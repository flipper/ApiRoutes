using ApiRoutes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SimpleNet70;

/// <summary>
/// Example Route
/// </summary>
/// <remarks>
/// Big Description for Example Route
/// </remarks>
[ApiRoute("/example", Method.POST)]
public class ExampleRoute : IRequest<ExampleRouteResponse>
{
    public string Test { get; set; }
}

public class ExampleRouteResponse
{
    
}

public class ExampleRouteHandler : IHandler<ExampleRoute, ExampleRouteResponse>
{
    public ValueTask<RequestResult> InvokeAsync(ExampleRoute request, CancellationToken cancellationToken = default)
    {
        return this.OkAsTask(new ExampleRouteResponse());
    }
}