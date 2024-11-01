using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    [Table("SystemUsers")]
    public class SystemUser : User
    {
        public string Permissions { get; set; }

        //public override void DisplayProfile()
        //{
        //    Console.WriteLine("System User PRofile");
        //}
    }
}
