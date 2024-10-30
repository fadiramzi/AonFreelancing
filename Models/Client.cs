using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{

    [Table("Clients")]
    public class Client : User
    {
        [Required]
        [MinLength(4, ErrorMessage = "اسم الشركة غير صالح")]
        public string CompanyName { get; set; }

        


        // Has many projects, 1-m
        public IEnumerable<Project>? Projects { get; set; }
  

        public override void DisplayProfile()
        {
            Console.WriteLine($"Client display profile, Company: {CompanyName}");
        }
    }
}
