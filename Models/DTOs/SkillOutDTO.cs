namespace AonFreelancing.Models.DTOs
{
    public class SkillOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        SkillOutDTO(Skill skill)
        {
            Id = skill.Id;
            Name = skill.Name;
        }
        public static SkillOutDTO FromSkill(Skill skill) => new SkillOutDTO(skill);
    }
}