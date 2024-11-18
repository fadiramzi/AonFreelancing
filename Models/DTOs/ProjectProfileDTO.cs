namespace AonFreelancing.Models.DTOs
{
    public class ProjectProfileDTO
    {


        public long Id { get; set; }

        public string Name { get; set; }    
        public string Description { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

    }
}
