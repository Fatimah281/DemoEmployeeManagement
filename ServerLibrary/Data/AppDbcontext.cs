using BaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Data
{
    public class AppDbcontext(DbContextOptions<AppDbcontext> options) : DbContext(options)
    {
        public DbSet<Employee> Employee { get; set; }
        public DbSet<GeneralDepartment> GeneralDepartment { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Branch> Branch { get; set; }
        public DbSet<Town> Town { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<SystemRole> SystemRole { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<refreshTokenInfo> RefreshTokenInfo { get; set; }

    }
}
