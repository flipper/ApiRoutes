using JetBrains.Annotations;

namespace ApiRoutes;

[UsedImplicitly]
public sealed class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
{
    private readonly Func<TRequest, CancellationToken, ValueTask<TResponse>> _handler;
    
    public RequestHandler(IRequestHandlerCore<TRequest, TResponse> handler, IEnumerable<IHandlerFilter<TRequest, TResponse>> filters)
    {
        Func<TRequest, CancellationToken, ValueTask<TResponse>> next = handler.InvokeAsync;

        foreach (var filter in filters)
        {
            next = new HandlerFilterRunner<TRequest, TResponse>(filter, next).GetDelegate();
        }
            

        _handler = next;
    }
    
    public ValueTask<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken = default) => _handler(request, cancellationToken);
}