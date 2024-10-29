﻿using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly MainAppContext _mainAppContext;

        public ClientsController(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        // CRUD 
        // Read Get all clients with thier own projects by using Query mode
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? Mode)
        {
            var ClientList = new List<ClientDTO>();
            if (Mode == null || Mode == "basic")
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
            if (Mode == "r")
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
                         ClientId = p.ClientId
                     })
                 })
                .ToListAsync();
            }

            return Ok(ClientList);
        }

        // Create new client and send it to the database 
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientInputDTO client)
        {
            Client c = new Client();
            c.Name = client.Name;
            c.Username = client.Username;
            c.CompanyName = client.CompanyName;
            c.Password = client.Password;

            await _mainAppContext.Clients.AddAsync(c); // Entity Async for add new item to the database 
            await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
            return Ok("created");
        }

        // get by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClients(int id)
        {
            // to get one client with all projects his own (by id)
            var c = new ClientDTO();
            c = await _mainAppContext.Clients
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
                        ClientId = p.ClientId
                    })
                }).FirstOrDefaultAsync(c => c.Id == id);

            return Ok(c);
        }

        // update Clients by id (update name, username, password, skills)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ClientInputDTO ClientDTO)
        {
            Client? c = await _mainAppContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (c != null)
            {
                c.Name = ClientDTO.Name; // update name
                c.Username = ClientDTO.Username; // update username
                c.Password = ClientDTO.Password; // update password
                c.CompanyName = ClientDTO.CompanyName; // update company name

                await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
                return Ok(c);

            }

            return NotFound();
        }


        // Delete Clients by id 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Client? f = await _mainAppContext.Clients.FirstOrDefaultAsync(f => f.Id == id);
            if (f != null)
            {
                _mainAppContext.Remove(f); // Entity for deletion.
                await _mainAppContext.SaveChangesAsync(); // saving the changes to the database
                return Ok("Deleted");

            }

            return NotFound();
        }
    }
}
