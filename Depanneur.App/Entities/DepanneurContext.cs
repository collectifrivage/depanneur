using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Depanneur.App.Entities
{
    public class DepanneurContext : IdentityDbContext<User>
    {
        public DepanneurContext(DbContextOptions<DepanneurContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasMany(x => x.Logins)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasMany(x => x.Roles)
                .WithOne()
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Purchase>()
                .HasOne(x => x.Product)
                .WithMany(x => x.Purchases)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payment>();
            builder.Entity<Adjustment>();

            builder.Entity<Subscription>()
                .HasIndex(x => new {x.UserId, x.ProductId})
                .IsUnique();
        }
        

        public async Task EnsureSeedData(IServiceScope serviceScope)
        {
            var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            foreach (var role in App.Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
