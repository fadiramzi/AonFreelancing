using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AonFreelancing.Interfaces;

namespace AonFreelancing.Models
{
    public abstract class User : IUserAuthOperations,IUserPrintOperations
    {
        public int UserId { get; set; }

        [StringLength(512)]
        public string Name { get; set; }

        [StringLength(256)]
        public string Username { get; set; }


        [JsonIgnore]
        public string Password { get; set; }
        public int Id { get; set; }

        //one to one relationships
        public Freelancer Freelancer { get; set; }
        public Client Client { get; set; }
        public SystemUser SystemUser { get; set; }

        public abstract void DisplayProfile();
       
        

        public void Login()
        {

        }


        public  void Logout()
        {

        }



    }
}
