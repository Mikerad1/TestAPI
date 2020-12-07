using MichaelWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MichaelWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration _configuration;

        private readonly string _APIURL;

        public HomeController(IConfiguration configuration,ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _APIURL = _configuration.GetSection("Settings:APIURL").Value;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username,string password)
        {
            var user = await PasswordSignInAsync(username,password);


            if (user.UserId != 0)
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(user.Token);
                var tokenS = handler.ReadToken(user.Token) as JwtSecurityToken;
                var authorizedRoles = tokenS.Claims.Where(claim => claim.Type.Contains("role")).ToList();

                var claims = new List<Claim>{
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim("bearer", user.Token),
                        new Claim("id", user.UserId.ToString())
                    };


                foreach (Claim role in authorizedRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Value)); //build claims using roles
                }


                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); //build identity using claims
                AuthenticationProperties authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true, //Refreshing the authentication session should be allowed.

                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(100),
                    // The time at which the authentication ticket expires. A
                    // value set here overrides the ExpireTimeSpan option of
                    // CookieAuthenticationOptions set with AddCookie.

                    IsPersistent = true,
                    // Whether the authentication session is persisted across
                    // multiple requests. When used with cookies, controls
                    // whether the cookie's lifetime is absolute (matching the
                    // lifetime of the authentication ticket) or session-based.

                    IssuedUtc = DateTime.Now, //The time at which the authentication ticket was issued.

                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http
                    // redirect response value.
                };

                await HttpContext.SignInAsync(
                           CookieAuthenticationDefaults.AuthenticationScheme,
                           new ClaimsPrincipal(claimsIdentity),
                           authProperties);




                return RedirectToAction("Index", "Home");
            }

            return View("Index");
        }

        public async Task<User> PasswordSignInAsync(string username,string password)
        {
            //The login button sends username and password to the api to authenticate the user
            var client = new RestClient(_APIURL + "/Users/");

            var request = new RestRequest("Authenticate/?username="+username+"&password="+password, Method.POST);
            //request.AddJsonBody(credentials);
            request.RequestFormat = DataFormat.Json;
            var response = await client.PostAsync<User>(request);

            return response;
        }
    }
}
