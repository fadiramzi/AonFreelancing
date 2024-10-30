﻿using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientDTO:UserOutDTO
    {
        public string CompanyName { get; set; }

        // Has many projects, 1-m
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }

    public class ClientInputDTO: UserDTO
    {
        [Required]
        [MinLength(4,ErrorMessage ="Invalid Company Name")]
        public string CompanyName { get; set; }
    }

    public class ClienOutDTO : UserOutDTO
    {
        [Required]
        [MinLength(4, ErrorMessage = "Invalid Company Name")]

        // Has many projects, 1-m
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
        public string CompanyName { get; set; }
    }
}
