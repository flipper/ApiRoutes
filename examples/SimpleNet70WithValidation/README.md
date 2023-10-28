# Simple .NET 7 with Validation

FluentValidation is already a dependency of ApiRoutes. Just add a class extending RequestValidator<TRequest>.
**Do not register** FluentValidation with any auto discovery. All registration of validators is handled with Source Generation.

Features:
* .NET 7
* FluentValidation