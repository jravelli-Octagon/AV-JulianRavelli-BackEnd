using Microsoft.AspNetCore.Authorization;
using AmericanVirtual.Weather.Challenge.Common.Extensions;

namespace AmericanVirtual.Weather.Challenge.WebAPI.Authorization
{
    public sealed class PermissionAuthorizationRequirement : AuthorizationHandler<PermissionAuthorizationRequirement>,
        IAuthorizationRequirement
    {
        public PermissionAuthorizationRequirement(IEnumerable<string> permissions)
        {
            Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        }

        public IEnumerable<string> Permissions { get; }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PermissionAuthorizationRequirement requirement)
        {
            if (context.User == null || requirement.Permissions == null || !requirement.Permissions.Any())
                return Task.CompletedTask;

            if (!context.User!.Identity!.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            var hasPermission =
                requirement.Permissions.Any(permission => context.User.HasPermission(permission));

            if (!hasPermission) return Task.CompletedTask;

            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}