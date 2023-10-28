# ApiRoutes

ApiRoutes is a lightweight library for simplifying the implementation of API routes and request handling in C# using Source Generators.

## Features

- Simplifies the definition and handling of API routes.
- Generates route code using Source Generators for improved developer experience.
- Supports request/response patterns for handling API requests.
- Full support for NativeAOT with .NET 8.
- Zero Overhead. Same performance as Minimal API. No reflection at runtime.

## CQRS Pattern

ApiRoutes follows the Command Query Responsibility Segregation (CQRS) pattern, which separates read and write operations into distinct parts. By adopting the CQRS pattern, you can achieve better scalability, performance, and maintainability in your applications.

The library encourages the use of the CQRS pattern by providing a clear separation between requests and handlers. Requests represent commands or queries, while handlers encapsulate the business logic to handle these requests.


## Getting Started

### Prerequisites

- .NET 7.0 or later

### Installation

To use the ApiRoutes Library in your project, follow these steps:

1. Install the NuGet package into your project:

   ```shell
   dotnet add package ApiRoutes

2. Define your request classes by creating classes that implement the IRequest<TResponse> interface and annotate them with the ApiRoute attribute. The ApiRoute attribute takes a route pattern and HttpMethod enum value from the library. For example:
   ```csharp
   [ApiRoute("/hello-world", Method.POST, ReadBody = true)]
   public class HelloWorldCommand : IRequest<string>
   {
        public string MyString { get; set; }
   }
   ```
3. Create the handlers for your requests by implementing the IHandler<TRequest, TResponse> interface. For example:
   ```csharp
   public class HelloWorldCommandHandler : IHandler<HelloWorldCommand, string>
   {
        public async ValueTask<RequestResult> InvokeAsync(HelloWorldCommand command, CancellationToken cancellationToken = default)
        {
            // Handle the request logic
   
            // Do not throw exceptions! Return them with this.Error(exception);
            // All exceptions should extend RequestException to specify their status code.
   
            return this.Ok("Hello World");
        }
   }
   ```
4. In your ASP.NET Core Minimal API application, add the following code to configure the ApiRoutes Library:
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   
   // Add ApiRoutes services
   builder.Services.AddApiRoutes<{AssemblyNameHere}GeneratedRouteConfiguration>();
   
   // ...
   
   var app = builder.Build();
   
   // Map ApiRoutes
   app.MapApiRoutes();
   
   // ...
   ```
5. Build and run your application.

For more detailed usage examples and advanced configurations, please refer to the [examples](./examples) directory in the project.