using ApiRoutes;
using ApiRoutes.Generated.SimpleNet70;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiRoutes<SimpleNet70GeneratedRouteConfiguration>();

var app = builder.Build();

app.MapApiRoutes();

app.Run();