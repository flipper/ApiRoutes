using System.Text.Json.Serialization;
using ApiRoutes;
using ApiRoutes.Generated.Swagger;
using ApiRoutes.Swagger;
using Swagger;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.AddConsole();

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.AddApiRoutes<SwaggerGeneratedRouteConfiguration>(configuration =>
{
    configuration.UseSwagger();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapApiRoutes();

app.UseSwagger();
app.UseSwaggerUIAotFriendly();

app.Run();

[JsonSerializable(typeof(ExampleRoute))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}