using System.Security.Claims;

namespace AmericanVirtual.Weather.Challenge.Common.Types
{
    public static class UserClaimTypes
    {
        public const string UserName = ClaimTypes.Name;
        public const string UserId = ClaimTypes.NameIdentifier;
        public const string SerialNumber = ClaimTypes.SerialNumber;
        public const string Permission = nameof(Permission);
        public const string PackedPermission = nameof(PackedPermission);
        public const string ImpersonatorUserId = nameof(ImpersonatorUserId);
    }
}