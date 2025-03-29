using Telegram.Bot;
using Telegram.Bot.Types;
using Zazagram.Abstractions;

namespace Zazagram.Modules;

public record Role(String name, List<User> users);

public static class RolesModule {
    private static List<Role> roles = [];
    public static void Register(TelegramBotClient bot) {
        Subscribe.OnMessage(bot, (msg) => msg.Text.StartsWith("/add_role"), async (ctx) => {
            var split = ctx.RecievedMessage.Text.Split(separator: ' ', count: 2, options: StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2) {
                await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Команда отправлена неверно");
                return;
            }
            roles.Add(new Role(split[1], []));
            await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Роль добавлена! Список ролей: " + String.Join(" ", roles));
        });

        Subscribe.OnMessage(bot, (msg) => msg.Text.StartsWith("/remove_role"), async (ctx) => {
            var split = ctx.RecievedMessage.Text.Split(separator: ' ', count: 2, options: StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2) {
                await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Команда отправлена неверно");
                return;
            }
            if (roles.Find(r => r.name == split[1]) is Role r) {
                roles.Remove(r);
                await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Роль удалена! Список ролей: " + String.Join(" ", roles));
            }
            else {
                await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Введенной роли не существует! Список ролей: " + String.Join(" ", roles));
            }
        });
    }
}
