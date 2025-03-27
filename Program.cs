// See https://aka.ms/new-console-template for more information
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!, cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnMessage += OnMessage;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();

async Task OnMessage(Message msg, UpdateType type) {
    if (msg.Text is null) return;
    Console.WriteLine($"Received {type} '{msg.Text}' in {msg.Chat}");
    await bot.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");
}
