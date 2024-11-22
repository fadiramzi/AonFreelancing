using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    [Table("Freelancers")]
    public class Freelancer : User
    {

        public string Skills { get; set; }

        public List<Bid>? Bids { get; set; } // One-Many Relation
        //public override void DisplayProfile()
        //{
        //    Console.WriteLine($"Overrided Method in Freelancer Class");
        //}

    }


}
