using AonFreelancing.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace AonFreelancing.Contexts
{
    public class MainAppContext(DbContextOptions<MainAppContext> contextOptions) 
        : IdentityDbContext<User, ApplicationRole, long>(contextOptions)
    {
        // For TPT design, no need to define each one
        //public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Project> Projects { get; set; }
        //public DbSet<Client> Clients { get; set; }

        // instead, use User only
        public new DbSet<User> Users { get; set; } // Will access Freelancers, Clients, SystemUsers through inheritance and ofType 
        public DbSet<Otp> Otps { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // For TPT design
            builder.Entity<User>().ToTable("AspNetUsers");
            builder.Entity<Freelancer>().ToTable("Freelancers");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<SystemUser>().ToTable("SystemUsers");

            builder.Entity<User>().HasOne<Otp>()
            .WithOne()
            .HasForeignKey<Otp>()
            .HasPrincipalKey<User>(nameof(User.PhoneNumber));

            base.OnModelCreating(builder);
        }

    }
}
