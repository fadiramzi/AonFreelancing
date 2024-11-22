﻿using AonFreelancing.Models;
using AonFreelancing.Models.Responses;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using Task = System.Threading.Tasks.Task;

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
        public DbSet<TemUser> TemUsers { get; set; }       
        public DbSet<User> Users { get; set; } // Will access Freelancers, Clients, SystemUsers through inheritance and ofType 
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<EntityTask> Tasks { get; set; }
     
        public DbSet<Bid>Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            // For TPT design

            builder.Entity<User>().ToTable("AspNetUsers")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<TempUser>().ToTable("TempUser")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<EntityTask>().ToTable("Tasks");
          
            builder.Entity<Freelancer>().ToTable("Freelancers");
            builder.Entity<Bid>().ToTable("Bids"); 
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<SystemUser>().ToTable("SystemUsers");
            builder.Entity<OTP>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE","length([Code]) = 6"));
            builder.Entity<TemUser>().ToTable("TempUsers").HasIndex(u=>u.phoneNumber).IsUnique();   
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
            
            base.OnModelCreating(builder);
        }
    }
}
