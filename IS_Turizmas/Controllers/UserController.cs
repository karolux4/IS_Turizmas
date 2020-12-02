using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IS_Turizmas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IS_Turizmas.Controllers
{
    public class UserController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly SignInManager<RegistruotiVartotojai> _signInManager;

        public UserController(ApplicationDbContext context, SignInManager<RegistruotiVartotojai> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("/");
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(username, password,false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Nepavyko prisijungti");
                    return View();
                }
            }
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult EditUser()
        {
            return View();
        }
        public IActionResult DeleteUser()
        {
            return View();
        }

        public IActionResult ChangePlan()
        {
            return View();
        }
    }
}
