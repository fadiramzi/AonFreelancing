using AonFreelancing.Models.Entities;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using static System.Net.WebRequestMethods;

namespace AonFreelancing.Contexts
{
    public class MainAppContext(DbContextOptions<MainAppContext> contextOptions) 
        : IdentityDbContext<UserEntity, ApplicationRoleEntity, long>(contextOptions)
    {
        // For TPT design, no need to define each one
        //public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<ProjectEntity> Projects { get; set; }
        //public DbSet<Client> Clients { get; set; }

        // instead, use User only
        public DbSet<UserEntity> Users { get; set; } // Will access Freelancers, Clients, SystemUsers through inheritance and ofType 
        public DbSet<OtpEntity> OTPs { get; set; }
        public DbSet<TempUserEntity> TempUsers { get; set; }
        public DbSet<BidsEntity> Bids { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            // For TPT design
            builder.Entity<UserEntity>().ToTable("AspNetUsers")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<TempUserEntity>().ToTable("TempUser")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            
            builder.Entity<FreelancerEntity>().ToTable("Freelancers");
            builder.Entity<ClientEntity>().ToTable("Clients");
            builder.Entity<SystemUserEntity>().ToTable("SystemUsers");
            builder.Entity<OtpEntity>().ToTable("Otps", o => o.HasCheckConstraint("CK_CODE","length([Code]) = 6"));

            //set up relationships
            builder.Entity<TempUserEntity>().HasOne<OtpEntity>()
                                    .WithOne()
                                    .HasForeignKey<OtpEntity>()
                                    .HasPrincipalKey<TempUserEntity>(nameof(TempUserEntity.PhoneNumber));

            builder.Entity<ProjectEntity>().ToTable("Projects", 
                tb => tb.HasCheckConstraint("CK_PRICE_TYPE", $"[PriceType] IN ('{Constants.PROJECT_PRICE_TYPE_FIXED}', " +
                $"'{Constants.PROJECT_PRICE_TYPE_PER_HOUR}')"));

            builder.Entity<ProjectEntity>()
                .ToTable("Projects", tb => tb.HasCheckConstraint("CK_QUALIFICATION_NAME", 
                "[QualificationName] IN ('uiux', 'frontend', 'mobile', 'backend', 'fullstack')"));

            builder.Entity<ProjectEntity>()
                .ToTable("Projects", tb => tb.HasCheckConstraint("CK_STATUS", $"[Status] IN ('{Constants.PROJECT_STATUS_AVAILABLE}', " +
                $"'{Constants.PROJECT_STATUS_CLOSED}')"))
                .Property(p=>p.Status).HasDefaultValue(Constants.PROJECT_STATUS_AVAILABLE);

            builder.Entity<ProjectEntity>().Property(p => p.PriceType)
                .HasDefaultValue(Constants.PROJECT_PRICE_TYPE_FIXED);

            builder.Entity<BidsEntity>()
               .HasOne(b => b.Project)
               .WithMany(p => p.Bids)
               .HasForeignKey(b => b.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BidsEntity>()
                .HasOne(b => b.Freelancer)
                .WithMany(f => f.Bids)
                .HasForeignKey(b => b.FreelancerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BidsEntity>().Property(b => b.Status)
                .HasDefaultValue(Constants.BID_STATUS_PENDING);

            builder.Entity<TaskEntity>().
                ToTable("Tasks", t => t.HasCheckConstraint("CK_TASK_STATUS", $"[Status] IN ('{Constants.TASK_STATUS_DONE}', " +
                $"'{Constants.TASK_STATUS_IN_REVIEW}', '{Constants.TASK_STATUS_IN_PROGRESS}', '{Constants.TASK_STATUS_TODO}')"));

            builder.Entity<TaskEntity>().Property(t => t.Status).HasDefaultValue(Constants.TASK_STATUS_TODO);

            base.OnModelCreating(builder);
        }
    }
}
