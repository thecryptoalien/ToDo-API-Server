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
                t.Property(p => p.CreateTime).HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
                t.Property(p => p.UpdateTime).HasDefaultValueSql("GETDATE()").ValueGeneratedOnAddOrUpdate();
            });

            base.OnModelCreating(builder);
        }
    }
}
