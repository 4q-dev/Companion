using Companion.Domain;
using Microsoft.EntityFrameworkCore;

namespace Companion.Usecase;

public class RoleManagement(CompanionDbContext dbContext) {
    public async Task<String> NewRole(Int64 chatId, String? input) {
        if (input == null) {
            return "";
        }
        var split = input.Split(' ', 2, StringSplitOptions.TrimEntries);
        if (split.Length < 2) {
            return "Комманда была введена неверно!";
        }
        var roleName = split[1];
        var roleExists = await dbContext.Roles
            .AnyAsync(r => r.TelegramChatId == chatId && r.Name == roleName);

        if (roleExists) {
            return $"Роль с именем '{roleName}' уже существует!";
        }

        dbContext.Roles.Add(new Role(Guid.NewGuid(), chatId, roleName));
        await dbContext.SaveChangesAsync();
        return $"Роль с именем '{roleName}' создана!";
    }

    public async Task<String> GetAllRoles(Int64 chatId) {
        var roles = await dbContext.Roles
            .Where(r => r.TelegramChatId == chatId)
            .ToListAsync();
        return $"Вот список всех ролей: \n{String.Join('\n', roles.Select(r => $"Имя: " + r.Name))}";
    }

    public async Task<String> RemoveRole(Int64 chatId, String? input) {
        if (input == null) {
            return "";
        }
        var split = input.Split(' ', 2, StringSplitOptions.TrimEntries);
        if (split.Length < 2) {
            return "Комманда была введена неверно!";

        }
        var roleName = split[1];
        var role = await dbContext.Roles
            .FirstOrDefaultAsync(r => r.TelegramChatId == chatId && r.Name == roleName);

        if (role == null) {
            return $"Роль '{roleName}' не найдена!";
        }

        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync();

        return $"Роль '{roleName}' успешно удалена!";
    }

    public async Task AddMember(Role role, User user) {
        if (!role.Members.Any(m => m.Id == user.Id)) {
            role.Members.Add(user);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveMember(Role role, User user) {
        var member = role.Members.FirstOrDefault(m => m.Id == user.Id);
        if (member != null) {
            role.Members.Remove(member);
            await dbContext.SaveChangesAsync();
        }
    }
}
