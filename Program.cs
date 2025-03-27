using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

LoggingConfigure.ConfigureLogging();

using var cts = new CancellationTokenSource();

var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!, cancellationToken: cts.Token);
var me = await bot.GetMe();

bot.OnMessage += async (Message msg, UpdateType type) => {
    if (msg.Text is null) return;
    Log.Information($"Received {type} '{msg.Text}' in {msg.Chat}");
    await bot.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");
};

Log.Information($"@{me.Username} is running... Press Enter to terminate");

Console.ReadLine();

cts.Cancel();
