using System.Text.Json;
using System.Text.Json.Serialization;
using ApiRoutes;
using ApiRoutes.Benchmarks;
using ApiRoutes.Generated.ApiRoutes.Benchmarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.TestHost;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmarks>();
    }
}
[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[RankColumn, MemoryDiagnoser]
public class Benchmarks
{
    private HttpClient _mediatorServer;
    //private HttpClient _mvcServer;
    private HttpClient _minimalApiServer;
    private HttpClient _fastEndpointsServer;

    private static readonly Request _request = new Request
    {
        Name = "Test"
    };

    private static readonly string _requestUri = "/test";
    private static readonly string _requestUri2 = "/test2";
    private static readonly AppJsonSerializerContext AppJsonSerializerContext = new AppJsonSerializerContext();

    [GlobalSetup]
    public void Setup()
    {
        _mediatorServer = CreateApiRoutesServer().CreateClient();
        //_mvcServer = CreateMvcServer().CreateClient();
        _minimalApiServer = CreateMinimalApiServer().CreateClient();
        _fastEndpointsServer = CreateFastEndpointsServer().CreateClient();
    }

    [Benchmark]
    public async Task<string> ApiRoute()
    {
        return await SendRequest(_mediatorServer, _requestUri);
    }
    
    [Benchmark(Baseline = true)]
    public async Task<string> ApiRouteWithClass()
    {
        return await SendRequest(_mediatorServer, _requestUri2);
    }

    /*[Benchmark]
    public async Task<string> Mvc()
    {
        return await SendRequest(_mvcServer, _requestUri);
    }*/
    
    [Benchmark]
    public async Task<string> MinimalApi()
    {
        return await SendRequest(_minimalApiServer, _requestUri);
    }
    
    [Benchmark]
    public async Task<string> FastEndpoints()
    {
        return await SendRequest(_fastEndpointsServer, _requestUri);
    }

    private static async Task<string> SendRequest(HttpClient httpClient, string requestUri)
    {
        var response = await httpClient.PostAsJsonAsync(requestUri, _request, new JsonSerializerOptions
        {
            TypeInfoResolver = AppJsonSerializerContext
        });
        var output = await response.Content.ReadAsStringAsync();
        return output;
    }

    public static TestServer CreateMvcServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
            })
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddControllers();
                
                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.AddContext<AppJsonSerializerContext>();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });

        return new TestServer(builder);
    }
    
    public static TestServer CreateMinimalApiServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
            })
            .ConfigureServices(services =>
            {
                services.AddRouting();
                
                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.AddContext<AppJsonSerializerContext>();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapPost("/test", Handler);
                });
            });

        return new TestServer(builder);
    }

    private static Results<Ok<Response>, BadRequest> Handler([Microsoft.AspNetCore.Mvc.FromBody] Request request)
    {
        return TypedResults.Ok(new Response { Message = $"Hello {request.Name}" });
    }

    public static TestServer CreateApiRoutesServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
            })
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddApiRoutes<ApiRoutesBenchmarksGeneratedRouteConfiguration>();
                
                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.AddContext<AppJsonSerializerContext>();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints => { endpoints.MapApiRoutes(); });
            });

        return new TestServer(builder);
    }
    
    public static TestServer CreateFastEndpointsServer()
    {
        var builder = new WebHostBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
            })
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddFastEndpoints();
                
                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.AddContext<AppJsonSerializerContext>();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();

                app.UseAuthorization();
                
                
                app.UseEndpoints(endpoints => { endpoints.MapFastEndpoints(); });
            });

        return new TestServer(builder);
    }
}

[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(TestApiRouteRequest))]
[JsonSerializable(typeof(TestApiRouteRequestClass))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(Ok))]
[JsonSerializable(typeof(FastEndpointRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}