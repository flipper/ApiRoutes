using System.Runtime.CompilerServices;

namespace ApiRoutes;

public readonly struct HandlerFilterRunner<TRequest, TResponse>
{
    private readonly IHandlerFilter<TRequest, TResponse> filter;
    private readonly Func<TRequest, CancellationToken, ValueTask<TResponse>> next;

    public HandlerFilterRunner(
        IHandlerFilter<TRequest, TResponse> filter,
        Func<TRequest, CancellationToken, ValueTask<TResponse>> next)
    {
        this.filter = filter;
        this.next = next;
    }

    public Func<TRequest, CancellationToken, ValueTask<TResponse>> GetDelegate() => InvokeAsync;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<TResponse> InvokeAsync(TRequest request, CancellationToken cancellationToken) => filter.InvokeAsync(request, this.next, cancellationToken);
}