using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ApiRoutes.Generated.ApiRoutes.Tests.Project;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests;

namespace ApiRoutes.Tests;

public class Endpoints : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public Endpoints(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Get()
    {
        var response = await _client.GetAsync(Routes.ExampleGetRoute);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task Post()
    {
        var response = await _client.PostAsJsonAsync(Routes.ExamplePostRoute, new ExamplePostRoute("test"));

        var body = await response.Content.ReadFromJsonAsync<ExamplePostRouteResponse>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("test", body.Test);
    }
    
    [Fact]
    public async Task PostWithRouteAndQuery()
    {
        var url = Routes.ExamplePostAdvancedRoute.Replace("{id}", "testId") + "?query=test";
        
        var response = await _client.PostAsJsonAsync(url, new ExamplePostAdvancedRoute("test", null, null));

        var body = await response.Content.ReadFromJsonAsync<ExamplePostAdvancedRouteResponse>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("test", body.Test);
        Assert.Equal("test", body.Query);
        Assert.Equal("testId", body.Id);
    }
}