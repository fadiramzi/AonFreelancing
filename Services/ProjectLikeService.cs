using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;

public class ProjectLikeService
{
    private readonly MainAppContext _context;

    public ProjectLikeService(MainAppContext mainAppContext)
    {
        _context = mainAppContext;
    }

    public async Task LikeProjectAsync(long userId, long projectId)
    {
        var existingLike = await _context.ProjectLikes.FirstOrDefaultAsync(pl => pl.UserId == userId && pl.ProjectId == projectId);

        if (existingLike != null)
        {
            throw new InvalidOperationException("User has already liked this project.");
        }

        var newProjectLike = new ProjectLike(userId, projectId);
        await _context.ProjectLikes.AddAsync(newProjectLike);
        await _context.SaveChangesAsync();
    }

    public async Task UnlikeProjectAsync(long userId, long projectId)
    {
        var projectLike = await _context.ProjectLikes
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.ProjectId == projectId);

        if (projectLike == null)
        {
            throw new InvalidOperationException("Like not found.");
        }

        _context.ProjectLikes.Remove(projectLike);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetLikesCountAsync(long projectId)
    {
        return await _context.ProjectLikes
            .Where(pl => pl.ProjectId == projectId)
            .CountAsync();
    }
}
