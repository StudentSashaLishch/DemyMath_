using Microsoft.AspNetCore.Identity;

namespace DemyMath.Infrastructure.Identity;

public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentitySeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        string[] roles = ["User", "Admin"];

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': " +
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}