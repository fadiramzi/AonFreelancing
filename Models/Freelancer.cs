using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    public class Freelancer : User
    {

        public string Skills { get; set; }


        public override void DisplayProfile()
        {
            Console.WriteLine($"Overrided Method in Freelancer Class")
        }

    }


}
