using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Services.Identity;

namespace SugarTalk.Core.Middlewares.Authorization;

public class AuthorizationMiddlewareSpecification<TContext> : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationMiddlewareSpecification(
        ICurrentUser currentUser,
        IIdentityService identityService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool ShouldExecute(TContext context, CancellationToken cancellationToken) => true;

    public async Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        var (requiredRoles, requiredPermissions) =
            _identityService.GetRolesAndPermissionsFromAttributes(context.Message.GetType());

        if (!requiredRoles.Any() && !requiredPermissions.Any())
            return;

        if (IsApiKeyRequest())
        {
            if (requiredRoles.Any() || !requiredPermissions.All(HasApiKeyPermission))
                throw new UnauthorizedAccessException();

            return;
        }

        var requiredClaims = requiredRoles.Concat(requiredPermissions).ToArray();
        if (!_currentUser.Id.HasValue ||
            !await _identityService.IsInRolesAsync(_currentUser.Id.Value, requiredClaims, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAccessException();
    }

    public Task Execute(TContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task AfterExecute(TContext context, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task OnException(Exception ex, TContext context)
    {
        ExceptionDispatchInfo.Capture(ex).Throw();
        throw ex;
    }

    private bool IsApiKeyRequest() =>
        _httpContextAccessor.HttpContext?.User.Identity?.AuthenticationType ==
        AuthenticationSchemeConstants.ApiKeyAuthenticationScheme;

    private bool HasApiKeyPermission(string permission) =>
        _httpContextAccessor.HttpContext?.User.Claims.Any(x =>
            x.Type == "permission" && x.Value == permission) == true;
}
