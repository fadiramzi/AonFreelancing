using AonFreelancing.Contexts;
using AonFreelancing.Models;

namespace AonFreelancing.Services
{
    public class ProjectLikeService(MainAppContext mainAppContext)
    {



        public async Task LikeProjectAsync(long userId,long projectId)
        {
            ProjectLike newProjectLike = new ProjectLike(userId, projectId);
            await mainAppContext.AddAsync(newProjectLike);
            await mainAppContext.SaveChangesAsync();
        }
        public async Task UnlikeProjectAsync(ProjectLike projectLike)
        {
            mainAppContext.ProjectLikes.Remove(projectLike);
            await mainAppContext.SaveChangesAsync();
        }



    }
}
