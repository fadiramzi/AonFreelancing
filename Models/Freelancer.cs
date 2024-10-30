using AonFreelancing.Models.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    [Table("Freelancers")]
    public class Freelancer : User
    {
        [Required]
        public string Skills { get; set; }

        public IEnumerable<ProjectOutDTO>? Projects { get; set; }

        public override void DisplayProfile()
        {
            Console.WriteLine($"Overrided Method in Freelancer Class");
        }

    }


}
