using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Interfaces
{
    public interface IStatisticsService
    {
        string CalculatePercentage(int total, int count);
    }
}
