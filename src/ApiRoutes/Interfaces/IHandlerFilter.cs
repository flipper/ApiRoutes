using JetBrains.Annotations;

namespace ApiRoutes;

[PublicAPI]
public interface IHandlerFilter<TRequest, TResponse>
{
    ValueTask<TResponse> InvokeAsync(TRequest request, Func<TRequest, CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken = default);
}