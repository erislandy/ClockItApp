using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CI.API.ViewModels;
using CI.DAL;
using CI.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public EmployeesController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET api/values http://localhost:5000/api/values
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            var employee = new User {
                UserName = model.Username,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(employee, model.Password);
            if(!result.Succeeded){
                return BadRequest(result);
            }
            return Ok(result);
        }

      
        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
         
         
    }
}
