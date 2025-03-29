using ResultSharp.Errors;
using ResultSharp.Extensions.FunctionalExtensions.Sync;
using Telegram.Bot;
using Telegram.Bot.Types;
using Zazagram.Abstractions;
using Zazagram.Services;

LoggingConfigure.ConfigureLogging();

var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("ZAZAGRAM_TOKEN")!);

Subscribe.OnMessage(bot, "/bebra", async (ctx) => {
    if (ctx.RecievedMessage is not null) {
        await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "бебра отправлена");
    }
});

//Subscribe.OnMessage(bot, (msg) => "penis", async (ctx) => {
//    var userId = ctx.RecievedMessage.From.Id;
//    var photos = await bot.GetUserProfilePhotos(userId);
//    var pp = photos.Photos;
//    foreach (var photo in pp) {
//        foreach (var photoPhoto in photo) {
//            var photoId = photoPhoto.FileId;
//            var file = await bot.GetFile(photoId);
//            await bot.SendPhoto(ctx.RecievedMessage.Chat.Id, file);
//        }
//    }
//});

Subscribe.OnMessage(bot, (msg) => msg.Text is String msgText ? msgText : Error.Failure(""), async (ctx) => {
    if (ctx.RecievedMessage?.Text is not null) {
        (await LlmService.Recognize(ctx.RecievedMessage.Text, [])).Match(
                async ok => await bot.SendMessage(ctx.RecievedMessage.Chat.Id, ok),
                async ok => await bot.SendMessage(ctx.RecievedMessage.Chat.Id, "Ошибка сосите")
        );
    }
});

Console.ReadLine();
