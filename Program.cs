using Abstractions;
using Telegram.Bot;

LoggingConfigure.ConfigureLogging();

var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!);

Subscribe.OnMessage(bot, "/bebra", async (ctx) => {
    await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "бебра отправлена");
});

Subscribe.OnMessage(bot, (msg) => "penis", async (ctx) => {
    await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "sex");
});

Console.ReadLine();
