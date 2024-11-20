using AonFreelancing.Models;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Contexts
{
    public class MainAppContext(DbContextOptions<MainAppContext> contextOptions) 
        : IdentityDbContext<User, ApplicationRole, long>(contextOptions)
    {
        public DbSet<Project> Projects { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Models.Task> Tasks{ get; set; }

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
            builder.Entity<OTP>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE","length([Code]) = 6"));

            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_PRICE_TYPE", "[PriceType] IN ('Fixed', 'PerHour')"));
            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_QUALIFICATION_NAME", "[QualificationName] IN ('uiux', 'frontend', 'mobile', 'backend', 'fullstack')"));
            builder.Entity<Project>().ToTable("Projects", tb => tb.HasCheckConstraint("CK_STATUS", "[Status] IN ('Available', 'Closed')"))
                .Property(p => p.Status).HasDefaultValue("Available");


            builder.Entity<Bid>().Property(b => b.Status).HasDefaultValue(Constants.BID_STATUS_PENDING);
            builder.Entity<Models.Task>().Property(t => t.Status).HasDefaultValue(Constants.TASK_STATUS_TODO);
            builder.Entity<Models.Task>().ToTable("Tasks",t=>t.HasCheckConstraint("CK_TASK_STATUS", $"[Status] IN ('{Constants.TASK_STATUS_DONE}', '{Constants.TASK_STATUS_IN_REVIEW}', '{Constants.TASK_STATUS_IN_PROGRESS}', '{Constants.TASK_STATUS_TODO}')"));



            //set up relationships
            builder.Entity<TempUser>().HasOne<OTP>()
                                    .WithOne()
                                    .HasForeignKey<OTP>()
                                    .HasPrincipalKey<TempUser>(nameof(TempUser.PhoneNumber));
             builder.Entity<Freelancer>().HasOne<Bid>()
                                    .WithOne()
                                    .HasForeignKey<Bid>()
                                    .HasPrincipalKey<Freelancer>(nameof(Freelancer.Id));




            base.OnModelCreating(builder);
        }
    }
}
