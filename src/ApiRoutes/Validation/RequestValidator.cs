using FluentValidation;
using JetBrains.Annotations;

namespace ApiRoutes;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class RequestValidator<T> : AbstractValidator<T> where T : IBaseRequest
{
    
}