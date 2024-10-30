using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Contexts
{
    public class MainAppContext:DbContext
    {
        public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Make sure each UserId for (Client, freelancer, systemUser) is unique
            modelBuilder.Entity<Freelancer>()
                .HasIndex(f => f.UserId)
                .IsUnique();
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.UserId)
                .IsUnique();
            modelBuilder.Entity<SystemUser>()
                .HasIndex(su => su.UserId)
                .IsUnique();
                
            // User and Freelancer one-to-one relationship
            modelBuilder.Entity<User>()
                .HasOne(u=> u.Freelancer)
                .WithOne(f=> f.User)
                .HasForeignKey<Freelancer>(f => f.UserId);
            // User and Client one-to-one relationship
            modelBuilder.Entity<User>()
                .HasOne(u=> u.Client)
                .WithOne(c => c.User)
                .HasForeignKey<Client>(c => c.UserId);
            // User and SystemUser one-to-one relationship
            modelBuilder.Entity<User>()
                .HasOne(user => user.SystemUser)
                .WithOne(systemUser => systemUser.User)
                .HasForeignKey<SystemUser>(systemUser => systemUser.UserId);
            // Client and projects one-to-many relationship
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Projects)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientId);
            // Freelancer and projects one-to-many relationship
            modelBuilder.Entity<Freelancer>()
                .HasMany(c => c.Projects)
                .WithOne(p => p.Freelancer)
                .HasForeignKey(p => p.FreelancerId);
        }
        public MainAppContext(DbContextOptions<MainAppContext> contextOptions) : base(contextOptions) {

        }

    }
}
