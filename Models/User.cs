using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AonFreelancing.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Models
{
    //Replaced with 'builder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();' in OnModelCreate
    //[Index(nameof(PhoneNumber), IsUnique = true)]

    public class User : IdentityUser<long>
    {
        public string Name { get; set; }
<<<<<<< HEAD

        public string About { get; set; }
=======
        public string? About {  get; set; }
<<<<<<< HEAD

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

=======
>>>>>>> bd49e789eca82cf0b70e0aad4d121920d1c2c3b2
>>>>>>> 7a1bf9d3c70dc397651dcc412af417235d1c26a5
    }
}
