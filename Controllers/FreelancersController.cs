using AonFreelancing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FreelancersController : ControllerBase
    {
        private static List<Freelancer> freelancerList = new List<Freelancer>();
        [HttpGet]
        public IActionResult GetAll()
        {
            
            return Ok(freelancerList);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Freelancer freelancer) { 
            freelancerList.Add(freelancer);
            return CreatedAtAction("Create", new { Id = freelancer.Id }, freelancerList);
        }

        [HttpGet("{id}")]
        public IActionResult GetFreelancer(int id)
        {
            Freelancer fr = freelancerList.FirstOrDefault(f => f.Id == id);

            if (fr == null)
            {
                return NotFound("The resoucre is not found!");
            }

            return Ok(fr);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Freelancer f = freelancerList.FirstOrDefault(f=>f.Id == id);
            if(f!= null)
            {
                freelancerList.Remove(f);
                return Ok("Deleted");

            }

            return NotFound();
        }



    }
}
