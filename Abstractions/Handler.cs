using System.Collections.Concurrent;
using ResultSharp.Core;
using ResultSharp.Errors;
using ResultSharp.Extensions.FunctionalExtensions.Sync;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Telegram.Bot.TelegramBotClient;

namespace Zazagram.Abstractions;

public interface IState { }

public class UserContext(TelegramBotClient botClient) {
    public List<Update> UpdateHistory { get; set; } = [];
    public Update RecievedUpdate => UpdateHistory.Last();
    public Message? RecievedMessage => UpdateHistory.FindLast(static (u) => u.Type == UpdateType.Message)?.Message;

    public List<IState> StateHistory { get; set; } = [];
    public IState CurrentState => StateHistory.Last();

    public TelegramBotClient BotClient = botClient;
}

public static class Subscribe {
    private static readonly ConcurrentDictionary<Int64, UserContext> ctxs = [];
    // абсолют сперма
    private static readonly ConcurrentQueue<(Func<Update, Result<UpdateType>>, OnUpdateHandler)> handlers = [];

    public static void OnMessage(TelegramBotClient client, String message, Func<UserContext, Task> handler) =>
        OnMessage(client, (_) => message, handler);

    public static void OnMessage(TelegramBotClient client, Func<Message, Result<String>> message, Func<UserContext, Task> handler) =>
        OnUpdate(client,
            (rupdate) => {
                if (rupdate.Type == UpdateType.Message) {
                    if (rupdate.Message!.Text != message(rupdate.Message!)) {
                        return Error.Failure();
                    }
                }
                return UpdateType.Message;
            },
            handler
        );

    public static void OnUpdate(TelegramBotClient client, Func<Update, Result<UpdateType>> subscriptionPredicate, Func<UserContext, Task> handler) {
        handlers.Enqueue((subscriptionPredicate, (recievedUpdate) => {
            static Result<Update> isUpdateTypeValid(Update? update, Func<Update, Result<UpdateType>> updateType) {
                if (update is Update u) {
                    var evaluatedUpdateType = updateType(update);
                    return evaluatedUpdateType.Then(updt => u.Type == updt ? Result.Success(u) : Error.Failure());
                }
                else {
                    return Error.Failure();
                }
            }

            UserContext generateCtx() {
                var userCtx = new UserContext(client);
                return userCtx;
            }

            isUpdateTypeValid(recievedUpdate, subscriptionPredicate)
                .Match(
                    async ok => {
                        var chatId = ExtractId(ok);
                        var ctx = ctxs.GetOrAdd(chatId, (_) => generateCtx());

                        ctx.UpdateHistory.Add(recievedUpdate);
                        await handler(ctx);
                    },
                    err => { }
                );

            return Task.FromResult(() => { });
        }
        ));
    }

    public static void SubscribeAll(TelegramBotClient client) {
        client.OnUpdate += (update) => {
            foreach (var (predicate, handler) in handlers) {
                var p = predicate(update);
                if (p.IsSuccess) {
                    var pred = p.Unwrap();
                    if (pred == update.Type) {
                        handler(update);
                        Console.WriteLine("bebra");
                        break;
                    }
                }
            }
            return Task.FromResult(() => { });
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
            _ => throw new Exception("Update doen't contain id")
        };
    }
}
