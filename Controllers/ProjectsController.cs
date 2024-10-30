using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;
        public ProjectsController( MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        // عرض جميع المشاريع
        [HttpGet]
        public IActionResult GetAll()
        {
            var project = _mainAppContext.Projects.ToList();
            return Ok(project);

        }

        // اضافة مشروع جديد مع اضافة معرف العميل ومعرف العامل المستقل
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectInputDTO projectInputDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            Project p = new Project
            {
                //Id = projectInputDTO.Id,
                Title = projectInputDTO.Title,
                Description = projectInputDTO.Description,
                ClientId = projectInputDTO.ClientId,
                FreelancerId = projectInputDTO.FreelancerId,

            };

            await _mainAppContext.Projects.AddAsync(p);
            await _mainAppContext.SaveChangesAsync();
            return Ok(p);
        }

        // عرض المشروع حسب المعرف
        [HttpGet("{id}")]
        public IActionResult GetProjectId(int id)
        {
            var project = _mainAppContext.Projects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound("لا يوجد مشروع لعرضة");
            }

            return Ok(project);
            
        }

        // تحديث المشروع
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectUpdate projectUpdate)
        {
            Project f = await _mainAppContext.Projects.FirstOrDefaultAsync(f => f.Id == id);


            if (f != null)
            {
                f.Title = projectUpdate.Title;
                f.Description = projectUpdate.Description;

                 await _mainAppContext.SaveChangesAsync();
                return Ok($"{id} : تم تحديث معلومات المشروع بنجاح المعرف  ");
            }

            return NotFound("! لا يوجد مشروع بهذا المعرف لتحديثة");

        }

        // حذف المشروع
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Project f = await _mainAppContext.Projects.FirstOrDefaultAsync(f => f.Id == id);

            if (f != null)
            {
                _mainAppContext.Remove(f);
                await _mainAppContext.SaveChangesAsync();

                return Ok($"{id} : تم حذف المشروع بنجاح ");
            }

            return NotFound("لا يوجد مشروع بهذا المعرف لحذفة");
        }

    }
}
