using Companion.Domain;
using ResultSharp.Core;
using ResultSharp.Errors;
using Microsoft.EntityFrameworkCore;

namespace Companion.Usecase;

public class RoleManagement(CompanionDbContext dbContext) {
    public async Task NewRole(Role role) {
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<Role>> GetAllRoles(Int64 chatId) {
        return await dbContext.Roles.Where(r => r.TelegramChatId == chatId).ToListAsync();
    }

    public async Task<Result<Role>> GetRole(String name, Int64 chatId) {
        return (await dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == name
                    && r.TelegramChatId == chatId)) is Role role ? role : Error.Failure();
    }

    public async Task RemoveRole(Role role) {

    }

    public async Task AddMember(Role role, User user) {

    }

    public async Task RemoveMember(Role role, User user) {

    }
}
