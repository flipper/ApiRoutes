using JetBrains.Annotations;

namespace ApiRoutes;

[PublicAPI]
public interface IRequest : IRequest<None>
{
    
}

// ReSharper disable once UnusedTypeParameter
[PublicAPI]
public interface IRequest<TResponse> : IBaseRequest<RequestResult> where TResponse : notnull
{
    
}

// ReSharper disable once UnusedTypeParameter
[PublicAPI]
public interface IBaseRequest<TResponse> : IBaseRequest where TResponse : notnull
{
    
}

[PublicAPI]
public interface IBaseRequest
{
    
}