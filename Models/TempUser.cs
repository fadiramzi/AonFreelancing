using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class TempUser
    {
        [Key]
        public long Id { get; set; }

        public string UserType { get; set; }
        public string PhoneNumber { get; set; }
        public bool verified { get; set; }
    }
}
