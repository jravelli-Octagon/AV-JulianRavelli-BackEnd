using AmericanVirtual.Weather.Challenge.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AmericanVirtual.Weather.Challenge.Common.Extensions
{
    public static class PrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            return principal.FindPermissions().Any(p => p.Equals(permission, StringComparison.OrdinalIgnoreCase));
        }

        public static IReadOnlyList<string> FindPermissions(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            List<string> permissions = principal.Claims
                .Where(c => c.Type.Equals(UserClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .ToList();

            IEnumerable<string> packedPermissions = principal.Claims.Where(c =>
                    c.Type.Equals(UserClaimTypes.PackedPermission, StringComparison.OrdinalIgnoreCase))
                .SelectMany(c => c.Value.UnpackFromString(":"));

            permissions.AddRange(packedPermissions);

            return permissions.AsReadOnly();
        }
    }
}