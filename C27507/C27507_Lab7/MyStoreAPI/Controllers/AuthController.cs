﻿using MyStoreAPI.Business;
using MyStoreAPI.DB;
using MyStoreAPI.Models;
using Core;

//JWT Authentication
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MyStoreAPI.Controllers{

    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase{

        private readonly IHostEnvironment hostEnvironment;

        public authController(IHostEnvironment hostEnvironment){
            this.hostEnvironment = hostEnvironment;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromBody] UserAccount userDataFromUI){

            if (userDataFromUI is null) return BadRequest("Invalid client request");            

            if (hostEnvironment.IsDevelopment()){
                this.mockDataUsers();

                //revisamos la lista de usuarios estatica y btenemos sus roles
                //bool isUserValid = User.allUsersData.FirstOrDefault(u => u.userName == loginUser.userName && u.userPassword == loginUser.userPassword);
                var isUserValid = false;
                var claimsRoleFromUser = new List<Claim>();
                foreach (var thisUser in UserAccount.allUsersData){

                    if(userDataFromUI.userName == thisUser.userName && userDataFromUI.userPassword == thisUser.userPassword){

                        claimsRoleFromUser.AddRange(thisUser.userRoles);
                        isUserValid = true;
                        break;
                    }                    
                }
                //si el usuario existe, creamos el token
                if(isUserValid == true){
                                                                            
                    //creamos el claim.Name con el nombre del usuario
                    var claims = new List<Claim>{
                        new Claim(ClaimTypes.Name, userDataFromUI.userName),                                
                    };
                    //pasamos los roles del usuario al claim general
                    claims.AddRange(claimsRoleFromUser);    

                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TheSecretKeyNeedsToBePrettyLongSoWeNeedToAddSomeCharsHere"));
                    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                    var tokeOptions = new JwtSecurityToken(
                        issuer: "https://localhost:7161",
                        audience: "https://localhost:7161",
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(30),
                        signingCredentials: signinCredentials
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                    Console.WriteLine(tokenString);
                    return Ok(new AuthenticatedResponse { Token = tokenString });
                }
            }
            return Unauthorized();
        }


        private void mockDataUsers(){

            if (!UserAccount.allUsersData.Any()){

                new UserAccount("varo", "123456", new List<Claim> {
                    new Claim(ClaimTypes.Name, "varo"),
                    new Claim(ClaimTypes.Role, "Admin")
                });

                new UserAccount("user2", "123", new List<Claim> {
                    new Claim(ClaimTypes.Name, "user2"),
                    new Claim(ClaimTypes.Role, "Operator")
                });

                new UserAccount("user3", "123", new List<Claim> {
                    new Claim(ClaimTypes.Name, "user3"),
                    new Claim(ClaimTypes.Role, "Customer")
                });

                new UserAccount("user4", "123", new List<Claim> {
                    new Claim(ClaimTypes.Name, "user4"),
                    new Claim(ClaimTypes.Role, "Customer")
                });

                new UserAccount("user5", "123", new List<Claim> {
                    new Claim(ClaimTypes.Name, "user5"),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.Role, "Customer")
                });
            }            
        }
    }

    public class LoginModel{
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class AuthenticatedResponse{
        public string? Token { get; set; }
    }

}
