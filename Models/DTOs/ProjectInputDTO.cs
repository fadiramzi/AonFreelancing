﻿using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectInputDTO
    {
        public string Title { get; set; }

       
        [AllowNull]
        public string Description { get; set; }

        public int ClientId { get; set; }//FK

        public int FreelancerId { get; set; }//FK
    }
}
