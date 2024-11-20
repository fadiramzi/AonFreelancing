using AonFreelancing.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace AonFreelancing.Contexts
{
<<<<<<< HEAD
    public class MainAppContext : IdentityDbContext<User, ApplicationRole, long>
=======
    public class MainAppContext(DbContextOptions<MainAppContext> contextOptions) 
        : IdentityDbContext<User, ApplicationRole, long>(contextOptions)
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
    {
        // For TPT design, no need to define each one
        //public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Project> Projects { get; set; }
        //public DbSet<Client> Clients { get; set; }

        // instead, use User only
        public DbSet<User> Users { get; set; } // Will access Freelancers, Clients, SystemUsers through inheritance and ofType 
        public DbSet<OTP> OTPs { get; set; }
<<<<<<< HEAD

        public DbSet<UsersTemp> UsersTemps { get; set; }
        public DbSet<TempOTP> TempOTPs { get; set; }
        //Add temp table for PhoneNumber verfiy 

        public MainAppContext(DbContextOptions<MainAppContext> contextOptions) : base(contextOptions)
        {

        }
=======
        public DbSet<TempUser> TempUsers { get; set; }
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2

        protected override void OnModelCreating(ModelBuilder builder)
        {

            // For TPT design
            builder.Entity<User>().ToTable("AspNetUsers")
<<<<<<< HEAD
                                        .HasIndex(u => u.PhoneNumber).IsUnique();
=======
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<TempUser>().ToTable("TempUser")
                .HasIndex(u=>u.PhoneNumber).IsUnique();
            
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
            builder.Entity<Freelancer>().ToTable("Freelancers");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<SystemUser>().ToTable("SystemUsers");
            builder.Entity<OTP>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE", "length([Code]) = 6"));
            builder.Entity<UsersTemp>().ToTable("UsersTemps");

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
