﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Twilio.Types;

namespace AonFreelancing.Models.DTOs
{
    public class UserDTO
    {
        [StringLength(64)]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(32)]
        public string PhoneNumber { get; set; }

        [MinLength(4,ErrorMessage ="Too short password")]
        public string Password { get; set; }


    }

    public class UserOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string PhoneNumber { get; set; }
        public string Email { get; set; }

    }

    public class UserResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string UserType { get; set; }
        public RoleResponseDTO Role { get; set; }
        public string About { get; set; }
    }
}
