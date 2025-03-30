namespace Companion.Domain;

public record class User(Guid Id, Int32 TelegramId, String? TelegramUserName, List<Role> Roles);

public record class Role(Guid Id, Int32 TelegramChatId, String Name, List<User> Members);
