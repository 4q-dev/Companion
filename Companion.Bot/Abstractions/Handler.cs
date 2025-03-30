using System.Collections.Concurrent;
using ResultSharp.Core;
using ResultSharp.Errors;
using ResultSharp.Extensions.FunctionalExtensions.Sync;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Telegram.Bot.TelegramBotClient;

namespace Bot.Abstractions;

public interface IState { }

public class UserContext(TelegramBotClient client, IServiceProvider serviceProvider) {
    public IServiceProvider ServiceProvider => serviceProvider;

    public List<Update> UpdateHistory { get; set; } = [];
    public Update RecievedUpdate => UpdateHistory.Last();
    public Message? RecievedMessage => UpdateHistory.FindLast(static (u) => u.Type == UpdateType.Message)?.Message;

    public List<IState> StateHistory { get; set; } = [];
    public IState CurrentState => StateHistory.Last();

    public TelegramBotClient BotClient => client;
}

public static class Subscribe {
    private static readonly ConcurrentDictionary<Int64, UserContext> ctxs = [];
    // абсолют сперма
    private static readonly ConcurrentQueue<(Func<Update, Result<UpdateType>> predicate, OnUpdateHandler handler)> handlers = [];

    public static IServiceProvider ServiceProvider { get; set; }

    public static void OnMessage(TelegramBotClient client, String message, Func<UserContext, Task> handler)
        => OnMessage(client, (_) => message, handler);

    public static void OnMessage(TelegramBotClient client, Func<Message, Boolean> isMessageValid, Func<UserContext, Task> handler)
        => OnUpdate(client,
            (recievedUpdate) => {
                if (recievedUpdate.Type == UpdateType.Message) {
                    if (!isMessageValid(recievedUpdate.Message!)) {
                        return Error.Failure();
                    }
                }
                return UpdateType.Message;
            },
            handler
        );

    public static void OnMessage(TelegramBotClient client, Func<Message, Result<String>> checkMessage, Func<UserContext, Task> handler)
        => OnUpdate(client,
            (recievedUpdate) => {
                if (recievedUpdate.Type == UpdateType.Message) {
                    if (recievedUpdate.Message!.Text != checkMessage(recievedUpdate.Message!)) {
                        return Error.Failure();
                    }
                }
                return UpdateType.Message;
            },
            handler
        );

    public static void OnUpdate(TelegramBotClient client, Func<Update, Result<UpdateType>> checkUpdateType, Func<UserContext, Task> handler) {
        Task Op(Update recievedUpdate) {
            static Result<Update> isUpdateTypeValid(Update? update, Func<Update, Result<UpdateType>> checkUpdateType) {
                return update is Update u
                    ? checkUpdateType(update)
                        .Then(checkedUpdateType => u.Type == checkedUpdateType ? Result.Success(u) : Error.Failure())
                    : Error.Failure();
            }

            UserContext generateCtx() {
                var userCtx = new UserContext(client, ServiceProvider);
                return userCtx;
            }

            isUpdateTypeValid(recievedUpdate, checkUpdateType)
                .Then(
                    async ok => {
                        var chatId = ExtractId(ok);
                        var ctx = ctxs.GetOrAdd(chatId, (_) => generateCtx());
                        ctx.UpdateHistory.Add(recievedUpdate);
                        await handler(ctx);
                    }
                );

            return Task.CompletedTask;
        }
    }
    // handlers.Enqueue((checkUpdateType, (recievedUpdate) => 
    // ));

    public static void SubscribeAll(TelegramBotClient client) {
        client.OnUpdate += static (update) => {
            foreach (var (predicate, handler) in handlers) {
                var p = predicate(update);
                if (p.IsSuccess) {
                    var pred = p.Unwrap();
                    if (pred == update.Type) {
                        handler(update);
                        break;
                    }
                }
            }
            return Task.FromResult(static () => { });
        };
    }

    private static Int64 ExtractId(Update update) {
        return update.Type switch {
            UpdateType.Message => update.Message!.Chat.Id,
            UpdateType.EditedMessage => update.EditedMessage!.Chat.Id,
            UpdateType.ChannelPost => update.ChannelPost!.Chat.Id,
            UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat.Id,
            UpdateType.EditedBusinessMessage => update.EditedBusinessMessage!.Chat.Id,
            UpdateType.DeletedBusinessMessages => update.DeletedBusinessMessages!.Chat.Id,
            UpdateType.MessageReaction => update.MessageReaction!.Chat.Id,
            UpdateType.MessageReactionCount => update.MessageReactionCount!.Chat.Id,
            UpdateType.MyChatMember => update.MyChatMember!.Chat.Id,
            UpdateType.ChatMember => update.ChatMember!.Chat.Id,
            UpdateType.ChatJoinRequest => update.ChatJoinRequest!.Chat.Id,
            UpdateType.ChatBoost => update.ChatBoost!.Chat.Id,
            UpdateType.RemovedChatBoost => update.RemovedChatBoost!.Chat.Id,
            _ => throw new ArgumentException("Update doen't contain id")
        };
    }
}
