using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiRoutes.EFCore;

public class DatabaseBehaviour<TRequest, TResponse> : IHandlerFilter<TRequest, RequestResult> where TRequest : IBaseRequest<RequestResult>
{
    private readonly DatabaseBehaviourConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseBehaviour<TRequest, TResponse>> _logger;

    public DatabaseBehaviour(DatabaseBehaviourConfiguration configuration, IServiceProvider serviceProvider, ILogger<DatabaseBehaviour<TRequest, TResponse>> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async ValueTask<RequestResult> InvokeAsync(TRequest request, Func<TRequest, CancellationToken, ValueTask<RequestResult>> next, CancellationToken cancellationToken = default)
    {
        if (!_configuration.Configurations.TryGetValue(typeof(TRequest), out var dbContextType))
        {
            return await next(request, cancellationToken);
        }

        if (_serviceProvider.GetService(dbContextType) is not DbContext dbContext)
        {
            return await next(request, cancellationToken);
        }
        
        var response = await next(request, cancellationToken);
        
        if (response.IsError)
        {
            return response;
        }

        _logger.LogInformation("Start DB Transaction");
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Commit DB Transaction");
        }
        catch (Exception)
        {
            _logger.LogInformation("Rollback DB Transaction");
            await transaction.RollbackAsync(cancellationToken);
            
            var handler = _serviceProvider.GetService(typeof(IHandler<TRequest, TResponse>));

            if (handler is IDatabaseTransactionHandler<TRequest> databaseTransactionHandler)
            {
                await databaseTransactionHandler.RollbackAsync(request, cancellationToken);
            }
            
            return new RequestResult(new RequestException(HttpStatusCode.InternalServerError, "Internal Server Error"));
        }
        
        return response;
    }
}