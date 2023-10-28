using Microsoft.AspNetCore.Mvc;

namespace ApiRoutes.Benchmarks;

public class TestController : Controller
{
    [HttpPost("/test")]
    public Task<Response> Greetings([FromBody] Request request)
    {
        return Task.FromResult(new Response
        {
            Message = $"Hello {request.Name}"
        });
    }
}