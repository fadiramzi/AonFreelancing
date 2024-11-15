
namespace AonFreelancing.Models.DTOs
{
    public class UserDetailsDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }

        public UserDetailsDTO(User user,string role)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            Role = role;
        }
    }
}