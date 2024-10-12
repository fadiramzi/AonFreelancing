using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    public class SystemUser : User
    {
        public string Permissions { get; set; }

        public override void DisplayProfile()
        {
            Console.WriteLine("System User PRofile");
        }
    }
}
