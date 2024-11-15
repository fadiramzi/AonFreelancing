using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AonFreelancing.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace AonFreelancing.Models
{
    //Replaced with 'builder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();' in OnModelCreate
    //[Index(nameof(PhoneNumber), IsUnique = true)]

    public class User : IdentityUser<long>
    {
        public string Name { get; set; }
        public string UserType { get; set; }

        public override string UserName { get; set; } = null; 
    }
}
