using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DemyMath.Infrastructure.Data;
using DemyMath.Infrastructure.Identity;

namespace DemyMath.Infrastructure.Identity;

public class IdentitySeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DemyMathDbContext _dbContext;

    public IdentitySeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        DemyMathDbContext dbContext)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _dbContext = dbContext;
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

        var usersWithoutRoles = await _dbContext.Users
            .Where(user => !_dbContext.UserRoles
                .Any(userRole => userRole.UserId == user.Id))
            .ToListAsync();

        foreach (var user in usersWithoutRoles)
        {
            var result = await _userManager.AddToRoleAsync(user, "User");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign User role to '{user.Id}': " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}