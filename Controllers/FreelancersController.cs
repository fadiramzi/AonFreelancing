using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/freelancers")]
    [ApiController]
    public class FreelancersController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public FreelancersController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }
        //عرض معلومات العاملين المستقلين
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? Mode)
        {
            var FreelancerList = new List<FreelancerDTO>();
            //number 0 -> عرض معلومات العمال فقط 
            if (Mode == null || Mode == "0")
            {
                FreelancerList = await _mainAppContext.Freelancers
                 .Include(c => c.Projects)
                  .Select(c => new FreelancerDTO
                  {
                      Id = c.Id,
                      Skills = c.Skills,
                      Name = c.Name,
                      Username = c.Username
                  })
                 .ToListAsync();
            }
            //number 1-> عرض معلومات العمال مع المشاريع المستلمة ان وجدت
            if (Mode == "1")
            {

                FreelancerList = await _mainAppContext.Freelancers
                .Include(c => c.Projects)

                 .Select(c => new FreelancerDTO
                 {
                     Id = c.Id,
                     Skills = c.Skills,
                     Name = c.Name,
                     Username = c.Username,
                     Projects = c.Projects.Select(p => new ProjectOutDTO
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Description = p.Description,
                         ClientId = p.ClientId,
                         FreelancerId = p.FreelancerId,

                     })
                 })
                .ToListAsync();
            }

            return Ok(FreelancerList);
        }

       //تسجيل عامل مستقل جديد
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] FreelancerInputDto freelancerInputDto)
        {
            ApiResponse<object> apiResponse;

            Freelancer f = new Freelancer
            {
                Name = freelancerInputDto.Name,
                Username = freelancerInputDto.Username,
                Password = freelancerInputDto.Password,
                Skills = freelancerInputDto.Skills
            };

            await _mainAppContext.Freelancers.AddAsync(f);
           await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };

            return Ok(apiResponse);
        }

        //عرض معلومات العامل حسب المعرف
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFreelancerId(int id)
        {

            Freelancer? fr = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);

            if (fr == null)
            {
                return NotFound(" ! لا يوجد عامل مستقل بهذا المعرف  ");
            }

            return Ok(fr);

        }

        //تحديث معلومات العمال المستقلين
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FreelancerUpdateDto freelancerUpdateDto)
        {
            Freelancer f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                f.Name = freelancerUpdateDto.Name;
                f.Username = freelancerUpdateDto.Username;
                f.Password = freelancerUpdateDto.password;
                f.Skills = freelancerUpdateDto.Skills;

               await _mainAppContext.SaveChangesAsync();
                return Ok($"{id} : تم تحديث معلومات العامل المستقل المعرف ");

            }

            return NotFound(" ! لا يوجد عامل مستقل بهذا المعرف ");
        }

        // حذف عامل مستقل
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Freelancer f = await _mainAppContext.Freelancers.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                _mainAppContext.Remove(f);
                await _mainAppContext.SaveChangesAsync();
                return Ok($"{id} : تم حذف العامل المستقل بنجاح معرف العامل ");

            }

            return NotFound("! لا يوجد عامل مستقل بهذا المعرف لحذفه ");
        }


    }
}
