using System.Text.Json.Serialization;
using ApiRoutes;
using FluentValidation;

namespace SimpleNet70;

/// <summary>
/// Example Route
/// </summary>
/// <remarks>
/// Big Description for Example Route
/// </remarks>
[ApiRoute("/example", Method.POST)]
public class ExampleRoute : IRequest
{
    /// <summary>
    /// Example String
    /// </summary>
    public string ExampleString { get; set; }
}

/// <summary>
/// Validators are added at compile time using Source Generators. No reflection at startup
/// </summary>
public class ExampleRouteValidator : RequestValidator<ExampleRoute>
{
    public ExampleRouteValidator()
    {
        RuleFor(x => x.ExampleString).NotEmpty().MinimumLength(100).MaximumLength(1000);
    }
}

public class ExampleRouteHandler : IHandler<ExampleRoute>
{
    public ValueTask<RequestResult> InvokeAsync(ExampleRoute request, CancellationToken cancellationToken = default)
    {
        return this.NoContentAsTask();
    }
}