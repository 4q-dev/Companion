using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Companion.Usecase;

public static class DiExtensions {
    public static IServiceCollection AddUsecaseServices(this IServiceCollection services) {
        services.AddDbContext<CompanionDbContext>(static options =>
            options.UseNpgsql(@"Host=localhost;Port=5432;Username=postgres;Password=example;Database=companion_db"));
        services.AddTransient<RoleManagment>();
        return services; // Возвращаем IServiceCollection для chain-вызовов
    }
}
