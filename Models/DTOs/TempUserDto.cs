using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows.Markup;
using  AonFreelancing.Utilities;

namespace AonFreelancing.Models.DTOs
{
    public class TempUserDTO
    {
        [Required]
        [AllowedValues(Constants.USER_TYPE_CLIENT, Constants.USER_TYPE_FREELANCER)]
        public string UserType { get; set; }

        [Required]
        [Length(14,14)]
        public string PhoneNumber { get; set; }        
    }

    public class TempUserOutDTO
    {
        public string UserType { get; set; }
        public string PhoneNumber { get; set; }
        public bool verified { get; set; }

    }
}
