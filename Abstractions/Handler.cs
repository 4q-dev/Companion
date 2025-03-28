using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Abstractions;

public interface IState { }

public class UserContext(TelegramBotClient botClient) {
    public List<Message> MessageHistory { get; set; } = [];
    public Message RecievedMessage => MessageHistory.Last();

    public List<IState> StateHistory { get; set; } = [];
    public IState CurrentState => StateHistory.Last();

    public TelegramBotClient BotClient = botClient;
}

public static class Subscribe {
    private static ConcurrentDictionary<Int64, UserContext> ctxs = [];

    public static void OnMessage(TelegramBotClient client, String message, Func<UserContext, Task> handler) =>
        OnMessage(client, (_) => message, handler);

    public static void OnMessage(TelegramBotClient client, Func<Message, String> message, Func<UserContext, Task> handler) {
        client.OnMessage += async (Message msg, UpdateType type) => {
            if (msg.Text is null || msg.Text != message(msg)) { return; }
            Log.Information($"Received {type} '{msg.Text}' in {msg.Chat}");

            var generateCtx = () => {
                var userCtx = new UserContext(client);
                return userCtx;
            };

            var ctx = ctxs[msg.Chat.Id] switch {
                null => generateCtx(),
                var userCtx => userCtx,
            };

            ctx.MessageHistory.Add(msg);

            await handler(ctx);
        };
    }
}
