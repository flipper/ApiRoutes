# Swagger Example

Uses .NET 8 and the new AOT feature to showcase the usage of ApiRoutes in a AOT deployment.

**ApiRoutes.Swagger** adds a new Swashbuckle.AspNetCore middleware to support AOT and a custom OperationFilter to support generating a Swagger document without reflection even at runtime while using AOT.


Features:
* .NET 8
* NativeAOT
* Swashbuckle.AspNetCore
* FluentValidation