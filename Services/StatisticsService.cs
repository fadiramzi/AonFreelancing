using AonFreelancing.Contexts;
using AonFreelancing.Interfaces;
using AonFreelancing.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class StatisticsService : IStatisticsService
    {
        public string CalculatePercentage(int total, int count)
        {
            if (total == 0) return "0%"; // Handle division by zero case
            return $"{Math.Round((double)count / total * 100, 2)}%"; // Calculate and format percentage
        }
    }
}
