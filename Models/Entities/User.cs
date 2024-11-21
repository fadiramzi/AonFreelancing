using Microsoft.AspNetCore.Identity;

namespace AonFreelancing.Models.Entities
{
    public class User : IdentityUser<long>
    {
        public string Name { get; set; }
        public string? About { get; set; }
    }
}
