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
        [MinLength(2, ErrorMessage = "يرجى ادخال مهارات حقيقية")]
        public string Skills { get; set; }

       

        public IEnumerable<Project>? Projects { get; set; }


        public override void DisplayProfile()
        {
            Console.WriteLine($"Overrided Method in Freelancer Class");
        }

    }


}
