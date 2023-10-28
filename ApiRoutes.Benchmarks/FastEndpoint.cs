using FastEndpoints;

namespace ApiRoutes.Benchmarks;


public class FastEndpointRequest
{
    public string Name { get; set; }
    
    public string QueryString { get; set; }
}

public class FastEndpoint : Endpoint<FastEndpointRequest>
{
    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("/test");
        AllowAnonymous();
    }

    public override Task HandleAsync(FastEndpointRequest request, CancellationToken ct)
    {
        return SendAsync(new Response
        {
            Message = $"Hello {request.Name}",
            QueryString = request.QueryString
        }, cancellation: ct);
    }
}