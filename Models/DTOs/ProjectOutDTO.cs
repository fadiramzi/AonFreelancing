﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectOutDTO
    {
        public int Id { get; set; }

        [StringLength(200)]
        [MinLength(2, ErrorMessage = "يرجى ادخال  عنوان صالح ")]
        public string Title { get; set; }
        public string Description { get; set; }

        public int ClientId { get; set; }

        public int FreelancerId { get; set; }

        DateTime CreatedAt { get; set; }
    }
}
