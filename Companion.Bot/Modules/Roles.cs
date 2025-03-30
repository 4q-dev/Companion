using Bot.Abstractions;
using Companion.Usecase;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Modules;

public static class RolesModule {
    public static void Register() {
        Subscribe.OnMessage(
                static (msg) => msg.Text is String text && text.StartsWith("/role_add", StringComparison.CurrentCulture),
                static async (TelegramBotClient bot, Message msg, RoleManagement roleManagment)
                    => await bot.SendMessage(msg.Chat.Id, await roleManagment.NewRole(msg.Chat.Id, msg.Text))
        );

        Subscribe.OnMessage(
                static (msg) => msg.Text is not null && msg.Text!.StartsWith("/role_remove", StringComparison.CurrentCulture),
                static async (TelegramBotClient bot, Message msg, RoleManagement roleManagment)
                    => await bot.SendMessage(msg.Chat.Id, await roleManagment.RemoveRole(msg.Chat.Id, msg.Text))
        );

        Subscribe.OnMessage(
                static (msg) => msg.Text is not null && msg.Text!.StartsWith("/role_desc", StringComparison.CurrentCulture),
                static async (TelegramBotClient bot, Message msg, RoleManagement roleManagment)
                    => await bot.SendMessage(msg.Chat.Id, await roleManagment.GetAllRoles(msg.Chat.Id))
        );

        // Subscribe.OnMessage(bot, (msg) => msg.Text is not null && msg.Text!.StartsWith("/role_sub", StringComparison.CurrentCulture), async (ctx) => {
        //     var split = ctx.RecievedMessage!.Text!.Split(' ', 3, StringSplitOptions.TrimEntries);
        //     if (split.Length < 3) {
        //         await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Команда отправлена неверно. `/role_sub @user role_name`");
        //         return;
        //     }
        //     var userName = split[1];
        //     var roleName = split[2];
        //     if (roles.Find(r => r.name == split[2]) is Role r) {
        //         r.Usernames.Add(userName);
        //         await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Пользователь добавлен в роль! Список ролей: " + String.Join(" ", roles));
        //     }
        //     else {
        //         await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Введенной роли не существует! Список ролей: " + String.Join(" ", roles));
        //     }
        // });
        //
        // Subscribe.OnMessage(bot, (msg) => msg.Text is not null && msg.Text!.StartsWith("/role_unsub", StringComparison.CurrentCulture), async (ctx) => {
        //     var split = ctx.RecievedMessage!.Text!.Split(' ', 3, StringSplitOptions.TrimEntries);
        //     if (split.Length < 3) {
        //         await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Команда отправлена неверно. `/role_sub @user role_name`");
        //         return;
        //     }
        //     var userName = split[1];
        //     var roleName = split[2];
        //     if (roles.Find(r => r.name == split[2]) is Role r) {
        //         r.Usernames.Remove(userName);
        //         await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Пользователь удален из роли! Список ролей: " + String.Join(" ", roles));
        //     }
        //     else {
        //         await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Пользователя нет в роли! Список ролей: " + String.Join(" ", roles));
        //     }
        // });
    }
}
