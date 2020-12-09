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
            if (_signInManager.IsSignedIn(User))
            {
                return LocalRedirect("/");
            }
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
            if (_signInManager.IsSignedIn(User))
            {
                return LocalRedirect("/");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Vardas, Pavarde, ElPastas, Slapyvardis, Slaptazodis")] RegistruotiVartotojai user, string passwordConfirmation, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("/");
            if (ModelState.IsValid)
            {
                if (!user.Slaptazodis.Equals(passwordConfirmation))
                {
                    ModelState.AddModelError("", "Slaptažodžiai nesutampa");
                    return View();
                }
                user.AktyvumoTaskai = 0;
                user.RegistracijosData = DateTime.Now;
                user.PrisijungimoData = DateTime.Now;
                var result = await _signInManager.UserManager.CreateAsync(user, user.Slaptazodis);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View();
        }

        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("/");
            await _signInManager.SignOutAsync();

            return LocalRedirect(returnUrl);
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
