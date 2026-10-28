using DemyMath.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Data;

public class DemyMathDbContext : IdentityDbContext<ApplicationUser>
{
    public DemyMathDbContext(
        DbContextOptions<DemyMathDbContext> options)
        : base(options)
    {
    }
}