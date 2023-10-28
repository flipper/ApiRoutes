namespace ApiRoutes.EFCore;

public interface IDatabaseTransactionHandler<in TRequest>
{
    Task RollbackAsync(TRequest request, CancellationToken cancellationToken = default);
}