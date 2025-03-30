namespace Companion.Domain;

public class User(Guid id, Int64 telegramId, String? telegramUserName) {
    public Guid Id { get; set; } = id;
    public Int64 TelegramId { get; set; } = telegramId;
    public String? TelegramUserName { get; set; } = telegramUserName;
    public List<Role> Roles { get; set; } = [];
}

public class Role(Guid id, Int64 telegramChatId, String name) {
    public Guid Id { get; set; } = id;
    public Int64 TelegramChatId { get; set; } = telegramChatId;
    public String Name { get; set; } = name;
    public List<User> Members { get; set; } = [];

    public override String? ToString() {
        return $"Id: {Id}\nName: {Name} Members: {String.Join(' ', Members.Select(static m => m.TelegramUserName))}\n\n\n";
    }
}
