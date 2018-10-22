using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Depanneur.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Depanneur.App.Data
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(DepanneurContext db) : base(db)
        {
        }

        public async Task<IDictionary<string, User>> GetUsersById(IEnumerable<string> userIds)
        {
            return await GetAll()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id)
                .ConfigureAwait(false);
        }

        public async Task<IDictionary<string, List<string>>> GetUsersRoles(IEnumerable<string> userIds)
        {
            var roles = await db.Roles.ToListAsync().ConfigureAwait(false);

            var users = await GetAll()
                .Include(x => x.Roles)
                .Where(x => userIds.Contains(x.Id))
                .ToListAsync()
                .ConfigureAwait(false);
            
            return users
                .ToDictionary(
                    x => x.Id,
                    x => x.Roles.Select(role => roles.First(r => r.Id == role.RoleId).Name).ToList()
                );
        }
    }
}