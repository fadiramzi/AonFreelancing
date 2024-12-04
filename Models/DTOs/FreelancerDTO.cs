﻿using AonFreelancing.Utilities;

namespace AonFreelancing.Models.DTOs
{
    //public class FreelancerDTO : UserDTO
    //{

    //    public string Skills { get; set; }
    //}

    //public class FreelancerRequestDTO : UserDTO
    //{
    //    public string Skills { get; set; }
    //}

    public class FreelancerResponseDTO : UserResponseDTO
    {
        public List<SkillOutDTO>Skills { get; set; }
        FreelancerResponseDTO(Freelancer freelancer)
        {
            Id = freelancer.Id;
            Name = freelancer.Name;
            Username = freelancer.UserName;
            PhoneNumber = freelancer.PhoneNumber;
            UserType = Constants.USER_TYPE_FREELANCER;
            IsPhoneNumberVerified = freelancer.PhoneNumberConfirmed;
            Role = new RoleResponseDTO { Name = Constants.USER_TYPE_FREELANCER };
            Skills = freelancer.Skills.Select(s => SkillOutDTO.FromSkill(s)).ToList();
        }
        public static FreelancerResponseDTO FromFreelancer(Freelancer freelancer)=>new FreelancerResponseDTO(freelancer);
    }

    public class FreelancerShortOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string QualificationName { get; set; }

        FreelancerShortOutDTO(Freelancer freelancer)
        {
            Id = freelancer.Id;
            Name = freelancer.Name;
        }
        public static FreelancerShortOutDTO FromFreelancer(Freelancer freelancer) => new FreelancerShortOutDTO(freelancer);
    }
}
