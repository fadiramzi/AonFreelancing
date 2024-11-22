using AonFreelancing.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
        public DbSet<User> Users { get; set; } // Will access Freelancers, Clients, SystemUsers through inheritance and ofType 
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            // For TPT design
            builder.Entity<User>().ToTable("AspNetUsers")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<TempUser>().ToTable("TempUser")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            
            builder.Entity<Freelancer>().ToTable("Freelancers");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<SystemUser>().ToTable("SystemUsers");
            builder.Entity<OTP>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE","LEN([Code]) = 6"));

            //set up relationships
            builder.Entity<TempUser>().HasOne<OTP>()
                                    .WithOne()
                                    .HasForeignKey<OTP>()
                                    .HasPrincipalKey<TempUser>(nameof(TempUser.PhoneNumber));

            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_PRICE_TYPE", "[PriceType] IN ('Fixed', 'PerHour')"));
            
            builder.Entity<Project>()
                .ToTable("Projects", tb => tb.HasCheckConstraint("CK_QUALIFICATION_NAME", "[QualificationName] IN ('uiux', 'frontend', 'mobile', 'backend', 'fullstack')"));
            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_STATUS", "[Status] IN ('Available', 'Closed')"))
                .Property(p=>p.Status).HasDefaultValue("Available");

            builder.Entity<Bid>()
               .HasOne(b => b.Project)
               .WithMany(p => p.Bids)
               .HasForeignKey(b => b.ProjectId)
               .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Bid>()
                .HasOne(b => b.Freelancer)
                .WithMany(f => f.Bids)
                .HasForeignKey(b => b.FreelancerId)
                .OnDelete(DeleteBehavior.NoAction);


            builder.Entity<TaskEntity>().ToTable("Tasks");

            base.OnModelCreating(builder);
        }
    }
}
