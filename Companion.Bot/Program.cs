using Bot.Abstractions;
using Bot.Modules;
using Companion.Usecase;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

LoggingConfigure.ConfigureLogging();

var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!);
bot.OnError += (error, source) => { throw error; };

var serviceBuilder = new ServiceCollection();
serviceBuilder.AddUsecaseServices();
Subscribe.ServiceProvider = serviceBuilder.BuildServiceProvider();

RolesModule.Register(bot);

Subscribe.OnMessage(bot, "/bebra", async (UserContext ctx) => {
    if (ctx.RecievedMessage is not null) {
        await bot.SendMessage(ctx.RecievedMessage.Chat.Id,
                String.Join(" ", ctx.UpdateHistory
                    .FindAll(u => u.Message?.Text is not null)
                    .Select(u => u.Message!.Text)));
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

Subscribe.SubscribeAll(bot);

Console.ReadLine();
