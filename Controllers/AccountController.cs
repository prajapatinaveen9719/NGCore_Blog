using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NGCore_Blog.Helpers;
using NGCore_Blog.Models;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NGCore_Blog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        /// <summary>
        /// /////////////////////////

        /// </summary>


        private readonly AppSettingsClass _appSetting;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,IOptions<AppSettingsClass>  appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSetting = appSettings.Value;
        }

        
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegistrationViewModel formData)
        {
            //hold all the error related to registration
            List<string> errorlist = new List<string>();
            var user = new IdentityUser
            {
                Email = formData.Email,
                UserName = formData.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(user, formData.Password);

            if (result.Succeeded)
            {
                //default role created of customer when result suceeded
                await _userManager.AddToRoleAsync(user, "Customer");
                //send Response back to angular application
                return Ok(new { username = user.UserName, email = user.Email, status = 1, message = "Registration Successful" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    //add this error to errorlist
                    errorlist.Add(error.Description);
                }
            }
            return BadRequest(new JsonResult(errorlist));
        }


        //Login Method
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel formdata)
        {
            //get user from database
            var user = await _userManager.FindByNameAsync(formdata.UserName);
            //get roles
            var roles = await  _userManager.GetRolesAsync(user);

            var tokenExpiryTime = Convert.ToDouble(_appSetting.ExpireTime);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSetting.Secret));

            if (user != null && await _userManager.CheckPasswordAsync(user, formdata.Password))
            {
                //confirmation of email

                //Generate Token
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, formdata.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                        new Claim("LoggedOn", DateTime.Now.ToString()),

                    }),
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSetting.Site,
                    Audience = _appSetting.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(new { token=tokenHandler.WriteToken(token),expiration=token.ValidTo,username=user.UserName,userRole=roles.FirstOrDefault()});
            }
            ModelState.AddModelError("", "UserName /Password was Not Found");
            return Unauthorized(new { LoginError = "Please Check The Credential Invalid Username Entered" });
         }

    }
}
