using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace ApiRoutes;

[PublicAPI]
public interface IPrepareRequest<TRequest>
{
    ValueTask<TRequest> PrepareAsync(HttpContext context, CancellationToken cancellationToken = default);
}