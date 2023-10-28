using ApiRoutes;
using ApiRoutes.Generated.SimpleNet70WithValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiRoutes<SimpleNet70WithValidationGeneratedRouteConfiguration>();

var app = builder.Build();

app.MapApiRoutes();

app.Run();