using System.Linq;
using System.Threading.Tasks;
using Depanneur.App.Data;
using Depanneur.App.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly UserRepository users;
    private readonly UserManager<User> userManager;

    public UserService(UserRepository users, UserManager<User> userManager)
    {
        this.users = users;
        this.userManager = userManager;
    }

    public async Task<User> SetRole(string userId, string roleName, bool assigned) 
    {
        var user = users.Get(userId);
        var hasRole = await userManager.IsInRoleAsync(user, roleName);

        if (hasRole && !assigned) 
        {
            await userManager.RemoveFromRoleAsync(user, roleName);
        }
        else if (!hasRole && assigned) 
        {
            await userManager.AddToRoleAsync(user, roleName);
        }

        return user;
    }
}