using Bot.Abstractions;
using Companion.Domain;
using Companion.Usecase;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot.Modules;

public static class RolesModule {
    public static void Register(TelegramBotClient bot) {
        Subscribe.OnMessage(
                bot,
                (msg) => msg.Text is String text
                    && text.StartsWith("/role_add", StringComparison.CurrentCulture),
                async (Message msg, RoleManagement roleManagment) => {
                    if (msg.Text is not null) {
                        var split = msg.Text.Split(' ', 2, StringSplitOptions.TrimEntries);
                        if (split.Length < 2) {
                            await bot.SendMessage(msg.Chat.Id, "Команда отправлена неверно");
                            return;
                        }
                        else {
                            await bot.SendMessage(msg.Chat.Id,
                                    "Список ролей: " + String.Join(' ', await roleManagment.GetAllRoles(msg.Chat.Id)));
                            await roleManagment.NewRole(new Role(Guid.NewGuid(), msg.Chat.Id, split[1]));
                            await bot.SendMessage(msg.Chat.Id,
                                    "Роль добавлена! Список ролей: \n" + String.Join(' ', await roleManagment.GetAllRoles(msg.Chat.Id)));
                        }
                    }
                }
        );

        // Subscribe.OnMessage(
        //         bot,
        //         (msg) => msg.Text is not null && msg.Text!.StartsWith("/role_remove", StringComparison.CurrentCulture),
        //         async (ctx) => {
        //             var split = ctx.RecievedMessage.Text.Split(' ', 2, StringSplitOptions.TrimEntries);
        //             if (split.Length < 2) {
        //                 await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Команда отправлена неверно");
        //                 return;
        //             }
        //             if (roles.Find(r => r.name == split[1]) is Role r) {
        //                 roles.Remove(r);
        //                 await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Роль удалена! Список ролей: " + String.Join(" ", roles));
        //             }
        //             else {
        //                 await bot.SendMessage(ctx.RecievedMessage!.Chat.Id, "Введенной роли не существует! Список ролей: " + String.Join(" ", roles));
        //             }
        //         }
        // );
        //
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
