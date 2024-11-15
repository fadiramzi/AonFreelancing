using AonFreelancing.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AonFreelancing.Contexts
{
    public class MainAppContext:IdentityDbContext<User, ApplicationRole, long>
    {
        // For TPT design, no need to define each one
        //public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Project> Projects { get; set; }
        //public DbSet<Client> Clients { get; set; }

        // instead, use User only
        public DbSet<User> Users { get; set; } // Will access Freelancers, Clients, SystemUsers through inheritance and ofType 
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<UserTemp> UserTemps { get; set; }
        public MainAppContext(DbContextOptions<MainAppContext> contextOptions) : base(contextOptions) {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            // For TPT design
            builder.Entity<User>().ToTable("AspNetUsers")
                                        .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<Freelancer>().ToTable("Freelancers");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<SystemUser>().ToTable("SystemUsers");
            builder.Entity<OTP>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE","length([Code]) = 6"));
            builder.Entity<UserTemp>().ToTable("Temp Users")
                                        .HasIndex(u => u.PhoneNumber)
                                        .IsUnique();

            //set up relationships
            //builder.Entity<User>().HasOne<OTP>()
            //                        .WithOne()
            //                        .HasForeignKey<OTP>()
            //                        .HasPrincipalKey<User>(nameof(User.PhoneNumber));


            base.OnModelCreating(builder);
        }

    }
}
