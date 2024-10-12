using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AonFreelancing.Interfaces;

namespace AonFreelancing.Models
{
    public abstract class User : IUserAuthOperations,IUserPrintOperations
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        protected int Id { get; set; }

        public abstract void DisplayProfile();
       
        

        public void Login()
        {

        }


        public  void Logout()
        {

        }



    }
}
