using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public ClientsController(MainAppContext mainAppContext) { 
            _mainAppContext = mainAppContext;
        }

        //عرض بيانات العميل
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? Mode) {
            var ClientList = new List<ClientDTO>();
            //basic -> عرض معلومات العملاء فقط
            if(Mode == null || Mode == "basic")
            {
                ClientList = await _mainAppContext.Clients
                 .Include(c => c.Projects)
                  .Select(c => new ClientDTO
                  {
                      Id = c.Id,
                      CompanyName = c.CompanyName,
                      Name = c.Name,
                      Username = c.Username
                  })
                 .ToListAsync();
            }
            // r -> عرض معلومات العملاء مع مشاريعهم المخلوقة أن وجدت
            if(Mode == "r")
            {
                ClientList = await _mainAppContext.Clients
                .Include(c => c.Projects)
                 .Select(c => new ClientDTO
                 {
                     Id = c.Id,
                     CompanyName = c.CompanyName,
                     Name = c.Name,
                     Username = c.Username,
                     Projects = c.Projects.Select(p => new ProjectOutDTO
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Description = p.Description,
                         ClientId = p.ClientId,
                         FreelancerId = p.FreelancerId

                     })
                 })
                .ToListAsync();
            }
           
            return Ok(ClientList);
        }

        //تسجيل عميل جديد
        [HttpPost("Register")]
        public async Task<IActionResult> Create([FromBody] ClientInputDTO clientInputDTO)
        {

            ApiResponse<object> apiResponse;

            Client f = new Client
            {
                Name = clientInputDTO.Name,
                Username = clientInputDTO.Username,
                CompanyName = clientInputDTO.CompanyName,
                Password = clientInputDTO.Password,

            };

            await _mainAppContext.Clients.AddAsync(f);
            await _mainAppContext.SaveChangesAsync();
            apiResponse = new ApiResponse<object>
            {
                IsSuccess = true,
                Results = f
            };

            return Ok(apiResponse);
        }

        //عرض معلومات العميل حسب المعرف
        [HttpGet("{id}")]
        public IActionResult GetClientId(int id)
        {
            Client? fr = _mainAppContext.Clients.FirstOrDefault(f => f.Id == id);
            if (fr == null)
            {
                return NotFound("لا يوجد عميل بهذا المعرف");
            }

            return Ok(fr);
        }

        //تحديث معلومات العميل 
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientInputDTO clientInputDTO)
        {
            Client f = await _mainAppContext.Clients.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                f.Name = clientInputDTO.Name;
                f.Username = clientInputDTO.Username;
                f.CompanyName = clientInputDTO.CompanyName;
                f.Password = clientInputDTO.Password;

                await _mainAppContext.SaveChangesAsync();
                return Ok($"{id} : تم تحديث معلومات العميل بنجاح المعرف ");
            }

            return NotFound("! لا يوجد عميل بهذا المعرف لتحديثة ");
        }

        //حذف عميل
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Client f = await _mainAppContext.Clients.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                _mainAppContext.Remove(f);
                await _mainAppContext.SaveChangesAsync();
                return Ok($"{id} : تم حذف العميل بنجاح المعرف ");
            }

            return NotFound("! لا يوجد عميل بهذا المعرف لحذفة ");
        }



    }
}
