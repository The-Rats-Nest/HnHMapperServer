using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Infrastructure.Identity;
using HnHMapperServer.Core.Constants;
using HnHMapperServer.Core.Extensions;
using Microsoft.AspNetCore.Http;

namespace HnHMapperServer.Api.Security;

/// <summary>
/// Custom claims principal factory that adds tenant context claims to the user principal.
/// Called by Identity when deserializing authentication cookies on both Web and API services.
/// </summary>
public class TenantClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantClaimsPrincipalFactory> _logger;

    public TenantClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        ApplicationDbContext db,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantClaimsPrincipalFactory> logger)
        : base(userManager, roleManager, optionsAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Check if a specific tenant was selected (set by SelectTenant endpoint via HttpContext.Items)
        var selectedTenantId = _httpContextAccessor.HttpContext?.Items["SelectedTenantId"] as string;

        // Load user's tenant assignment — use selected tenant if specified, otherwise first approved tenant
        var tenantUser = selectedTenantId != null
            ? await _db.TenantUsers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && tu.TenantId == selectedTenantId && tu.JoinedAt != default)
            : await _db.TenantUsers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && tu.JoinedAt != default);

        if (tenantUser != null)
        {
            // Add tenant context claims
            identity.AddClaim(new Claim(AuthorizationConstants.ClaimTypes.TenantId, tenantUser.TenantId));
            identity.AddClaim(new Claim(AuthorizationConstants.ClaimTypes.TenantRole, tenantUser.Role.ToClaimValue()));

            // Add standard Role claim for IsInRole() checks
            identity.AddClaim(new Claim(ClaimTypes.Role, tenantUser.Role.ToClaimValue()));

            // Load and add permission claims
            var permissions = await _db.TenantPermissions
                .IgnoreQueryFilters()
                .Where(tp => tp.TenantUserId == tenantUser.Id)
                .Select(tp => tp.Permission)
                .ToListAsync();

            foreach (var permission in permissions)
            {
                identity.AddClaim(new Claim(AuthorizationConstants.ClaimTypes.TenantPermission, permission.ToClaimValue()));
            }

            _logger.LogDebug("Added tenant claims for user {UserId}: TenantId={TenantId}, Role={Role}, Permissions={PermCount}",
                user.Id, tenantUser.TenantId, tenantUser.Role.ToClaimValue(), permissions.Count);
        }
        else
        {
            _logger.LogWarning("No tenant found for user {UserId}", user.Id);
        }

        return identity;
    }
}
