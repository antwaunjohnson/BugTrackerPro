using BugTrackerPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BugTrackerPro.Services.Factories;

public class BTProUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTProUser, IdentityRole>
{
    public BTProUserClaimsPrincipalFactory(UserManager<BTProUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            IOptions<IdentityOptions> optionsAccessor)
        :base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTProUser user)
    {
        ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()!));
        return identity;
    }
}
