using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using ResultSharp.Core;
using ResultSharp.Errors;
using ResultSharp.Extensions.FunctionalExtensions.Sync;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Telegram.Bot.TelegramBotClient;

namespace Bot.Abstractions;

public interface IState {
    void PreEnterState(UserContext ctx) { }
    void PreExitState(UserContext ctx) { }
}

public class UserContext(TelegramBotClient client, ServiceProvider serviceProvider) {
    public IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    // надо как то контролировать рост этого парня
    public List<Update> UpdateHistory { get; set; } = [];
    public Update RecievedUpdate => UpdateHistory.Last();
    public Message? RecievedMessage => UpdateHistory.FindLast(static (u) => u.Type == UpdateType.Message)?.Message;

    // public List<IState> StateHistory { get; set; } = [];
    public List<IState> currentStates { get; set; } = [];

    public TelegramBotClient BotClient => client;
}

public static class Subscribe {
    private static readonly TelegramBotClient client = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!);
    private static readonly ConcurrentDictionary<Int64, UserContext> ctxs = [];
    // абсолют сперма
    private static readonly ConcurrentQueue<(Func<Update, Result<UpdateType>> predicate, OnUpdateHandler handler)> handlers = [];

    public static ServiceProvider ServiceProvider { get; set; } = null!;

    public static void OnMessage(String message, Delegate handler)
        => OnMessage((_) => message, handler);

    public static void OnMessage(Func<Message, Boolean> isMessageValid, Delegate handler)
        => OnUpdate((recievedUpdate) => {
            if (recievedUpdate.Type == UpdateType.Message) {
                if (!isMessageValid(recievedUpdate.Message!)) {
                    return Error.Failure();
                }
            }
            return recievedUpdate.Type;
        },
            handler
        );

    public static void OnMessage(Func<Message, Result<String>> checkMessage, Delegate handler)
        => OnUpdate(
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

    public static void OnUpdate(Func<Update, Result<UpdateType>> checkUpdateType, Delegate handler) {
        Task Beb(Update recievedUpdate) {
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
                .OnSuccess(
                    ok => {
                        Task.Run(async () => {
                            using var scope = ServiceProvider.CreateScope();

                            var chatId = ExtractId(ok);
                            var ctx = ctxs.GetOrAdd(chatId, (_) => generateCtx());
                            ctx.ServiceProvider = scope.ServiceProvider;
                            ctx.UpdateHistory.Clear(); // хз нужно ли вообще хранить историю апдейтов так что пока так
                            ctx.UpdateHistory.Add(recievedUpdate);

                            var serviceTypes = handler.Method.GetParameters()
                               .Select(p => p.ParameterType);
                            var services = serviceTypes.Select((type) => ResolveFromContainer(type, ctx));
                            var task = handler.DynamicInvoke(services.ToArray()) switch {
                                Task t => t,
                                _ => Task.CompletedTask
                            };
                            await task;
                        });
                    }
                );

            return Task.CompletedTask;
        }

        handlers.Enqueue((checkUpdateType, Beb));
    }

    public static void SubscribeAll() {
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

    private static Object? ResolveFromContainer(Type serviceType, UserContext ctx) {
        if (serviceType == typeof(UserContext)) { return ctx; }
        if (serviceType == typeof(Message)) { return ctx.RecievedMessage; }
        if (serviceType == typeof(StateManager)) { return new StateManager(ctx); }

        return ctx.ServiceProvider.GetService(serviceType) is Object obj ? obj :
                throw new ArgumentNullException(nameof(serviceType));
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

public class StateManager(UserContext ctx) {
    public void EnterState(IState state) {
        state.PreEnterState(ctx);
        ctx.currentStates.Add(state);
    }
    public void ExitState(IState state) {
        state.PreExitState(ctx);
        ctx.currentStates.Remove(state);
    }
}
