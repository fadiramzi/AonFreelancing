using AonFreelancing.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private static List<Client> ClientList = new List<Client>();
        [HttpGet]
        public ActionResult<List<Client>> Get() =>
            ClientList;

        [HttpPost]
        public IActionResult Post([FromBody] Client client)
        {
            ClientList.Add(client);
            return Created();
        }
    }
}
