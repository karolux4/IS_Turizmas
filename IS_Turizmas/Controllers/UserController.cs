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
        private readonly RoleManager<VartotojoPlanai> _roleManager;

        public UserController(ApplicationDbContext context, SignInManager<RegistruotiVartotojai> signInManager,
            RoleManager<VartotojoPlanai> roleManager)
        {
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }


        public async Task<IActionResult> Index()
        {
            var userId = _signInManager.UserManager.GetUserId(User);
            if (userId == null)
            {
                return LocalRedirect("/");
            }

            int id = int.Parse(userId);
            RegistruotiVartotojai user = await _context.RegistruotiVartotojai.Include(o => o.VartotojoPlanai).ThenInclude(o => o.TipasNavigation).FirstOrDefaultAsync(o => o.Id == id);

            ViewBag.activityLevel = _context.AktyvumoLygiai.Where(o => o.Nuo <= user.AktyvumoTaskai && (o.Iki == null || o.Iki >= user.AktyvumoTaskai)).FirstOrDefault();

            return View(user);
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
                    RegistruotiVartotojai user = _context.RegistruotiVartotojai.Where(o => o.Slapyvardis == username).FirstOrDefault();
                    if(user.PrisijungimoData.Date != DateTime.Now.Date)
                    {
                        AddActivityPoints(5, user);
                    }

                    user.PrisijungimoData = DateTime.Now;
                    _context.SaveChangesAsync();
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
                    VartotojoPlanai plan = new VartotojoPlanai();
                    plan.DataNuo = DateTime.Now;
                    plan.Tipas = _context.VartotojoPlanoTipai.Where(o => o.Name == "nemokamas").FirstOrDefault().Id;
                    plan.FkRegistruotasVartotojas = user.Id;
                    var role = await _roleManager.CreateAsync(plan);
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

        public async Task<IActionResult> EditUser(int id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return View(await _context.RegistruotiVartotojai.Include(o => o.VartotojoPlanai).ThenInclude(o => o.TipasNavigation).FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserData(int id, [Bind("Id, Vardas, Pavarde, ElPastas," +
            " GimimoData")] EditViewRegistruotiVartotojai user, string Slaptazodis, string oldPassword, string passwordConfirmation)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            Console.WriteLine("1");

            RegistruotiVartotojai currentUser = _context.RegistruotiVartotojai.Find(id);

            currentUser.Vardas = user.Vardas;
            currentUser.Pavarde = user.Pavarde;
            currentUser.ElPastas = user.ElPastas;
            currentUser.GimimoData = user.GimimoData;
            

            if (!ModelState.IsValid)
            {
                return View("~/Views/User/EditUser.cshtml");
            }

            Console.WriteLine("2");

            if(!(Slaptazodis == null && oldPassword == null && passwordConfirmation == null))
            {
                Console.WriteLine("3");

                if(Slaptazodis == null || oldPassword == null || passwordConfirmation == null)
                {
                    Console.WriteLine("4");
                    ModelState.AddModelError("", "Turi būti nurodyti slaptažodžio laukai");
                    return View("~/Views/User/EditUser.cshtml");
                }

                if (!Slaptazodis.Equals(passwordConfirmation))
                {
                    Console.WriteLine("5");
                    ModelState.AddModelError("", "Slaptažodžiai nesutampa");
                    return View("~/Views/User/EditUser.cshtml");
                }

                if (!await _signInManager.UserManager.CheckPasswordAsync(currentUser, oldPassword))
                {
                    Console.WriteLine("5");
                    ModelState.AddModelError("", "Neteisingas slaptažodžis");
                    return View("~/Views/User/EditUser.cshtml");
                }

                var passwordValidator = new PasswordValidator<RegistruotiVartotojai>();

                var result = await passwordValidator.ValidateAsync(_signInManager.UserManager, null, Slaptazodis);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("~/Views/User/EditUser.cshtml");
                }

                var newPasswordHash = _signInManager.UserManager.PasswordHasher.HashPassword(currentUser, Slaptazodis);
                currentUser.Slaptazodis = newPasswordHash;
            }

            try
            {
                _context.Update(currentUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RegistruotiVartotojai.Any(o => o.Id == user.Id))
                {
                    return NotFound();
                }
                throw;
            }
            TempData["SuccessMessage"] = "Profilis atnaujintas";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return View(await _context.RegistruotiVartotojai.Include(o => o.VartotojoPlanai).ThenInclude(o => o.TipasNavigation).FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        public async Task<IActionResult> DeleteUserData(int id)
        {
            if (_signInManager.IsSignedIn(User) && int.Parse(_signInManager.UserManager.GetUserId(User)) == id)
            {
                //Vartotojo prenumeratos, bei ji uzprenumerave
                _context.Prenumeratos.RemoveRange(_context.Prenumeratos.Where(o => o.FkPrenumeratorius == id || o.FkPrenumeruojamasis == id));
                //Trinami vartotojo palikti komentarai
                _context.Komentarai.RemoveRange(_context.Komentarai.Where(o => o.FkRegistruotasVartotojas == id));
                //Trinami vartotojo palikti retingai
                _context.Reitingai.RemoveRange(_context.Reitingai.Where(o => o.FkRegistruotasVartotojas == id));
                //Trinami vartotojo planai
                _context.VartotojoPlanai.RemoveRange(_context.VartotojoPlanai.Where(o => o.FkRegistruotasVartotojas == id));
                //Trinami pasiulymu pranesimai
                _context.PasiulymuPranesimai.RemoveRange(_context.PasiulymuPranesimai.Where(o => o.FkRegistruotasVartotojas == id));


                //Trinami verslo vartotojai
                var versloVartotojas = _context.VersloVartotojai.FirstOrDefault(o => o.FkRegistruotasVartotojas == id);

                var versloReklamos = _context.Reklamos.Where(o => o.FkVersloVartotojas == id).ToList<Reklamos>();
                foreach(var reklama in versloReklamos)
                {
                    var reklamosPlanai = _context.ReklamosPlanai.Where(o => o.FkReklama == reklama.Id).ToList<ReklamosPlanai>();
                    foreach(var planas in reklamosPlanai)
                    {
                        //Istrinami atitinkamos reklamos plano laikai.
                        _context.ReklamavimoLaikai.RemoveRange(_context.ReklamavimoLaikai.Where(o => o.FkReklamosPlanas == planas.Id));
                    }

                    //Istrinami atitinkami reklamos planai.
                    _context.ReklamosPlanai.RemoveRange(reklamosPlanai);
                }

                _context.Reklamos.RemoveRange(versloReklamos);
                _context.VersloVartotojai.Remove(versloVartotojas);

                //Vartotojo marsrutai
                var marsrutai = _context.Marsrutai.Where(o => o.FkRegistruotasVartotojas == id).ToList<Marsrutai>();
                foreach(var marsrutas in marsrutai)
                {
                    _context.MarsrutoObjektai.RemoveRange(_context.MarsrutoObjektai.Where(o => o.FkMarsrutas == marsrutas.Id));
                }

                _context.Marsrutai.RemoveRange(marsrutai);
                RemoveAllInnactiveVisitableObjects();
                RemoveAllInnactiveCountries();

                //Istrinamas registruotas vartotojas
                _context.RegistruotiVartotojai.Remove(_context.RegistruotiVartotojai.Find(id));
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Profilis pašalintas";
                return LocalRedirect("/");
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        private async void RemoveAllInnactiveVisitableObjects()
        {
            _context.LankytiniObjektai.RemoveRange(_context.LankytiniObjektai.Where(o => o.MarsrutoObjektai.Count == 0));
            await _context.SaveChangesAsync();
        }

        private async void RemoveAllInnactiveCountries()
        {
            _context.Valstybes.RemoveRange(_context.Valstybes.Where(o => o.LankytiniObjektai.Count == 0));
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> ChangePlan(int id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return View(await _context.RegistruotiVartotojai.Include(o => o.VartotojoPlanai).ThenInclude(o => o.TipasNavigation).FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        public void AddActivityPoints(int points, RegistruotiVartotojai user)
        {
            user.AktyvumoTaskai += points;
        }
    }
}
