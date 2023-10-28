using ApiRoutes;
using ApiRoutes.Generated.SimpleNet70Form;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiRoutes<SimpleNet70FormGeneratedRouteConfiguration>();

var app = builder.Build();

app.MapApiRoutes();

app.Run();