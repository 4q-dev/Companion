using Companion.Domain;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace Companion.Usecase;

public class RoleManagement(CompanionDbContext dbContext, TelegramBotClient bot) {
    public async Task NewRole(Int64 chatId, String? input) {
        if (input == null) {
            return;
        }
        var split = input.Split(' ', 2, StringSplitOptions.TrimEntries);
        if (split.Length < 2) {
            await bot.SendMessage(chatId, "Комманда была введена неверно!");
            return;
        }
        var roleName = split[1];
        var roleExists = await dbContext.Roles
            .AnyAsync(r => r.TelegramChatId == chatId && r.Name == roleName);

        if (roleExists) {
            await bot.SendMessage(chatId, $"Роль с именем '{roleName}' уже существует!");
            return;
        }

        dbContext.Roles.Add(new Role(Guid.NewGuid(), chatId, roleName));
        await dbContext.SaveChangesAsync();
        await bot.SendMessage(chatId, $"Роль с именем '{roleName}' создана!");
    }

    public async Task GetAllRoles(Int64 chatId) {
        var roles = await dbContext.Roles
            .Where(r => r.TelegramChatId == chatId)
            .ToListAsync();
        await bot.SendMessage(chatId, $"Вот список всех ролей: {String.Join('\n', roles.Select(r => $"Имя: " + r.Name))}");
    }

    public async Task RemoveRole(Int64 chatId, String? input) {
        if (input == null) {
            return;
        }
        var split = input.Split(' ', 2, StringSplitOptions.TrimEntries);
        if (split.Length < 2) {
            await bot.SendMessage(chatId, "Комманда была введена неверно!");
            return;
        }
        var roleName = split[1];
        var role = await dbContext.Roles
            .FirstOrDefaultAsync(r => r.TelegramChatId == chatId && r.Name == roleName);

        if (role == null) {
            await bot.SendMessage(chatId, $"Роль '{roleName}' не найдена!");
            return;
        }

        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync();

        await bot.SendMessage(chatId, $"Роль '{roleName}' успешно удалена!");
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
