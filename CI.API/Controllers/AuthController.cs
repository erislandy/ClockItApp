using System.Text;
using System.Security.Claims;
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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace CI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _singInManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<User> userManager, 
                              SignInManager<User> singInManager,
                              IConfiguration config)
        {
            _userManager = userManager;
            _singInManager = singInManager;
            _config = config;
        }

        // POST api/auth/login/
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if(user == null) return BadRequest();
            var result = await _singInManager.CheckPasswordSignInAsync(user, model.Password, false);
            
            if(!result.Succeeded){
                return BadRequest(result);
            }
            return Ok(new {
                result,
                token = JwtTokenGeneratormachine(user)
            });
        }
        
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

    
            if(user != null && user.EmailConfirmed)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                /*
                var changePasswordUrl = Request.Headers["changePasswordUrl"];//http://localhost:4200/change-password

                var uriBuilder = new UriBuilder(changePasswordUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["token"] = token;
                query["userid"] = user.Id;
                uriBuilder.Query = query.ToString();
                var urlString = uriBuilder.ToString();

                var emailBody = $"Click on link to change password </br>{urlString}";
                 Not implemented sending email
                await _email.Send(model.Email, emailBody, _emailOptions.Value);

                return Ok()
                */
                 return Ok(new {
                     token,
                     UserId = user.Id
                });
            }
           return Unauthorized();
        }
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var resultPasswordConfirm = await _userManager.ResetPasswordAsync(user, Uri.UnescapeDataString(model.Token), model.Pass);
            if(resultPasswordConfirm.Succeeded)
            {
               
                return Ok();
            }
            return Unauthorized();
        }
       
        private async Task<string> JwtTokenGeneratormachine(User userInfo){

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
                new Claim(ClaimTypes.Name, userInfo.UserName)
            };

            var roles = await _userManager.GetRolesAsync(userInfo);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var securityKey =  new SymmetricSecurityKey(Encoding.ASCII
                                .GetBytes(_config.GetSection("AppSettings:Key").Value));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
         
    }
}
