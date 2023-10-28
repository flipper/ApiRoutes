using ApiRoutes;
using ApiRoutes.Generated.ApiRoutes.Tests.Project;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiRoutes<ApiRoutesTestsProjectGeneratedRouteConfiguration>();


var app = builder.Build();

app.MapApiRoutes();

app.Run();


public partial class Program { }