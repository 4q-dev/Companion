using Bot.Abstractions;
using Bot.Modules;
using Companion.Usecase;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

LoggingConfigure.ConfigureLogging();

var serviceBuilder = new ServiceCollection();
serviceBuilder.AddUsecaseServices();
serviceBuilder.AddSingleton(static _ => new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!));
Subscribe.ServiceProvider = serviceBuilder.BuildServiceProvider();

RolesModule.Register();

Subscribe.OnMessage("/bebra", static async (TelegramBotClient bot, UserContext ctx) => {
    if (ctx.RecievedMessage is not null) {
        await bot.SendMessage(ctx.RecievedMessage.Chat.Id,
                String.Join(" ", ctx.UpdateHistory
                    .FindAll(static u => u.Message?.Text is not null)
                    .Select(static u => u.Message!.Text)));
        await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "бебра отправлена");
    }
});

// Subscribe.OnMessage(bot, (msg) => msg.Text is String msgText ? msgText : Error.Failure(""), async (ctx) => {
//     if (ctx.RecievedMessage?.Text is not null) {
//         (await LlmService.Recognize(ctx.RecievedMessage.Text, [])).Match(
//             async ok => await bot.SendMessage(ctx.RecievedMessage.Chat.Id, ok),
//             async err => await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "Ошибка сосите")
//         );
//     }
// });

Subscribe.SubscribeAll();

Console.ReadLine();
