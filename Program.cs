using Abstractions;
using Telegram.Bot;

LoggingConfigure.ConfigureLogging();

var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!);

Subscribe.OnMessage(bot, "/add_role", async (ctx) => {
    await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "sex");
});

Subscribe.OnMessage(bot, (msg) => "penis", async (ctx) => {
    await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "sex");
});

Console.ReadLine();
