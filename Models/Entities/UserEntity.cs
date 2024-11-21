using Microsoft.AspNetCore.Identity;

namespace AonFreelancing.Models.Entities
{
    public class UserEntity : IdentityUser<long>
    {
        public string Name { get; set; }
        public string? About { get; set; }
        public List<BidsEntity> Bids { get; set; }
    }
}
