using Companion.Usecase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DiExtensions {
    public static IServiceCollection AddUsecaseServices(this IServiceCollection services) {
        services.AddDbContext<CompanionDbContext>(static options =>
            options.UseNpgsql(@"Host=localhost;Username=postgres;Password=example;Database=companion_db"));
        services.AddTransient<RoleManagment>();
        return services; // Возвращаем IServiceCollection для chain-вызовов
    }
}
