using Microsoft.EntityFrameworkCore;

namespace ApiRoutes.EFCore;


public abstract class HandlerWithDatabase<TRequest, TDbContext> : HandlerWithDatabase<TRequest, None, TDbContext> where TRequest : IBaseRequest<RequestResult> where TDbContext : DbContext
{
    public HandlerWithDatabase(TDbContext dbContext) : base(dbContext)
    {
    }
}

public abstract class HandlerWithDatabase<TRequest, TResponse, TDbContext> : IDatabaseTransactionHandler<TRequest>, IHandler<TRequest, TResponse> where TRequest : IBaseRequest<RequestResult> where TDbContext : DbContext
{
    public HandlerWithDatabase(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected TDbContext DbContext { get; }

    public virtual Task RollbackAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public abstract ValueTask<RequestResult> InvokeAsync(TRequest request,
        CancellationToken cancellationToken = new CancellationToken());
}
