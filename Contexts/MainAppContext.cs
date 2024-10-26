using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Contexts
{
    public class MainAppContext:DbContext
    {
        public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Client> Clients { get; set; }
        public MainAppContext(DbContextOptions<MainAppContext> contextOptions) : base(contextOptions) {

        }

    }
}
