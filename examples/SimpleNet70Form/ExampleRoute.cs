using System.Net;
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
    [FromForm] public string Text { get; set; }

    [FromForm] public decimal Rating { get; set; }
    
    [FromForm] public IFormFile RequiredFile { get; set; }

    [FromQuery]
    public bool Bool { get; set; }
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