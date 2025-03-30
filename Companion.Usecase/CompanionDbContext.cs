using Companion.Domain;
using Microsoft.EntityFrameworkCore;

namespace Companion.Usecase;

public class CompanionDbContext(DbContextOptions<CompanionDbContext> options) : DbContext(options) {
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;

}
