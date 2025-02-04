using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToDo_API_Server.Models;

namespace ToDo_API_Server.Data
{
    /// <summary>
    /// ApplicationDbContext - Database Context for ToDo API Server
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        /// <summary>
        /// ApplicationDbContext constructor that accepts DbContextOptions parameter
        /// </summary>
        /// <param name="options">DbContextOptions<ApplicationDbContext> object</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// ToDoEntry DbSet ToDoEntries
        /// </summary>
        public DbSet<ToDoEntry> ToDoEntries { get; set; }

        /// <summary>
        /// OnModelCreating protected override that accepts ModelBuilder parameter
        /// </summary>
        /// <param name="builder">ModelBuilder</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ToDoEntry>(t =>
            {
                // Set Database Default Sql value and when generated
                t.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
                t.Property(p => p.CreateTime).HasDefaultValueSql("now()");;
            });

            base.OnModelCreating(builder);

#if DEBUG
            SeedNewData(builder);
#endif
        }

        private void SeedNewData(ModelBuilder builder)
        {
            // Seed database
            string adminId = Guid.NewGuid().ToString();
            string roleId = Guid.NewGuid().ToString();
            builder.Entity<IdentityRole>().HasData(new IdentityRole { Id = roleId, Name = "Admin", NormalizedName = "admin" });

            var passwordHasher = new PasswordHasher<IdentityUser>();
            string adminEmail = "admin@email.com";
            var adminUser = new IdentityUser
            {
                Id = adminId,
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                UserName = adminEmail,
                NormalizedUserName = adminEmail.ToUpper(),
                EmailConfirmed = true
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "YourPassword123!");
            builder.Entity<IdentityUser>().HasData(adminUser);

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string> { RoleId = roleId, UserId = adminId });
        }
    }
}
