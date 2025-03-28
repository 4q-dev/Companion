using Abstractions;
using Serilog;
using Telegram.Bot;

LoggingConfigure.ConfigureLogging();

using var cts = new CancellationTokenSource();

var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!, cancellationToken: cts.Token);
var me = await bot.GetMe();

Subscribe.OnMessage(bot, "/add_role", async (ctx) => {
    await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "sex");
});


Subscribe.OnMessage(bot, (msg) => "penis", async (ctx) => {
    await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "sex");
});

Log.Information($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();

cts.Cancel();
