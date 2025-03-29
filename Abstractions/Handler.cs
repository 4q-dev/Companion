using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ResultSharp.Core;
using ResultSharp.Errors;
using ResultSharp.Extensions.FunctionalExtensions.Sync;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Abstractions;

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
    private static ConcurrentDictionary<Int64, UserContext> ctxs = [];

    public static void OnMessage(TelegramBotClient client, String message, Func<UserContext, Task> handler) =>
        OnMessage(client, (_) => message, handler);

    public static void OnMessage(TelegramBotClient client, Func<Message, Result<String>> update, Func<UserContext, Task> handler) =>
        OnUpdate(client,
            (rupdate) => {
                if (rupdate.Type == UpdateType.Message) {
                    update(rupdate.Message!);
                }
                return UpdateType.Message;
            },
            handler
        );

    public static void OnUpdate(TelegramBotClient client, Func<Update, Result<UpdateType>> subscriptionPredicate, Func<UserContext, Task> handler) {
        client.OnUpdate += (Update recievedUpdate) => {
            static Result<Update> isUpdateTypeValid(Update? update, Func<Update, Result<UpdateType>> updateType) {
                if (update is null) {
                    return Error.Failure();
                }
                var evaluatedUpdateType = updateType(update);
                if (evaluatedUpdateType.IsFailure) {
                    return Error.Failure();
                }
                return update.Type == evaluatedUpdateType.Unwrap() ? Result.Success(update) : Error.Failure();
            }

            UserContext generateCtx() {
                var userCtx = new UserContext(client);
                return userCtx;
            }

            isUpdateTypeValid(recievedUpdate, subscriptionPredicate)
                .Match(
                    async ok => {
                        var chatId = ExtractId(ok);
                        var ctx = ctxs.TryGetValue(chatId, out var userCtx) switch {
                            false => generateCtx(),
                            true => userCtx,
                        };

                        ctx.UpdateHistory.Add(recievedUpdate);
                        await handler(ctx);
                    },
                    err => { }
                );

            return Task.FromResult(() => { });
        };
    }

    public static Int64 ExtractId(Update update) {
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
