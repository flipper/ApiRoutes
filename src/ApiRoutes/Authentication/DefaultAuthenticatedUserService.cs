using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ApiRoutes.Authentication;

public sealed class DefaultAuthenticatedUserService : IAuthenticatedUserService
{
    public string UserId { get; }

    public DefaultAuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
    {
        UserId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}