using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Depanneur.App.Entities
{
    public class User : IdentityUser, ISoftDeletable
    {
        public string Name { get; set; }

        public decimal Balance { get; set; }

        public bool IsDeleted { get; set; }

        
        public virtual ICollection<IdentityUserRole<string>> Roles { get; } = new List<IdentityUserRole<string>>();
        public virtual ICollection<IdentityUserLogin<string>> Logins { get; } = new List<IdentityUserLogin<string>>();
    }
}
