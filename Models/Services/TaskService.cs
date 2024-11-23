using AonFreelancing.Contexts;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace AonFreelancing.Models.Services
{
    public class TaskService(MainAppContext mainAppContext,UserManager<User>userManager)
    {

        public double GetPrecentgeOfTask(int id)
        {
            var project =  mainAppContext.Projects.Where(p => p.Id == id).FirstOrDefault();
            if (project != null) { 
                var toatalTask = project.Tasks.Count();
            var completeTasks = project.Tasks.Count(t => t.Status.ToLower() == ConstantStatus.Status_done.ToLower());
            var percetge = toatalTask > 0 ? (completeTasks / toatalTask) : 0;

            return percetge;
        }
            return 0;
        }


     
    }
}
