using CABlazorApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CABlazorApp.Data
{
    public class ApplicationDbContext : IdentityDbContext

    {
        public DbSet<Users> users { get; set; }
        public DbSet<Certificate> certificates { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
    }
}