using JetBrains.Annotations;

namespace ApiRoutes;

public interface IRequestHandler
{
    
}

public interface IRequestHandlerCore<in TRequest, TResponse> : IRequestHandler
{
    ValueTask<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IRequestHandler<in TRequest, TResponse> : IRequestHandlerCore<TRequest, TResponse>
{
    
}

public interface IHandler<in TRequest> : IHandler<TRequest, None> where TRequest : IBaseRequest<RequestResult>
{
    
}

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IHandler<in TRequest, TResponse> : IRequestHandlerCore<TRequest, RequestResult>
    where TRequest : IBaseRequest<RequestResult>
{
}