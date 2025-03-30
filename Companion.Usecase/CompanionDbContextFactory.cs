using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Companion.Usecase;

public class CompanionDbContextFactory : IDesignTimeDbContextFactory<CompanionDbContext> {
    public CompanionDbContext CreateDbContext(String[] args) {
        var optionsBuilder = new DbContextOptionsBuilder<CompanionDbContext>();
        optionsBuilder.UseNpgsql(@"Host=localhost;Username=postgres;Password=example;Database=companion_db");
        return new CompanionDbContext(optionsBuilder.Options);
    }
}
