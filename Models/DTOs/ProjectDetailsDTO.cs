namespace AonFreelancing.Models.DTOs;

public class ProjectDetailsDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

}