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
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<ProjectHistory> ProjectHistories { set; get; }
        public MainAppContext(DbContextOptions<MainAppContext> contextOptions) : base(contextOptions) {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            // For TPT design
            builder.Entity<User>().ToTable("AspNetUsers")
                                        .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<Freelancer>().ToTable("Freelancers");
            builder.Entity<Client>().ToTable("Clients");
            builder.Entity<ProjectHistory>().ToTable("ProjectHistoys");
            builder.Entity<SystemUser>().ToTable("SystemUsers");
            builder.Entity<TempUser>().ToTable("TempUsers")
                                        .HasIndex(u=>u.PhoneNumber).IsUnique();
            builder.Entity<OTP>().ToTable("otps", o => o.HasCheckConstraint("CK_CODE","length([Code]) = 6"));

            //set up relationships
            builder.Entity<User>().HasOne<OTP>()
                                    .WithOne()
                                    .HasForeignKey<OTP>()
                                    .HasPrincipalKey<User>(nameof(User.PhoneNumber));
            builder.Entity<Client>()
                .HasMany(c => c.Projects)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade); 

            builder.Entity<Project>()
                .HasOne(p => p.ProjectHistory) 
                .WithOne(h => h.Project) 
                .HasForeignKey<ProjectHistory>(h => h.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);


            base.OnModelCreating(builder);
        }

    }
}
