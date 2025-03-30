using Companion.Domain;
using Microsoft.EntityFrameworkCore;

namespace Companion.Usecase;

public class CompanionDbContext : DbContext {
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;

    public CompanionDbContext(DbContextOptions<CompanionDbContext> options) : base(options) {
        Database.EnsureCreated();
    }
}
