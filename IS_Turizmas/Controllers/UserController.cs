using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using IS_Turizmas.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MailKit.Net.Smtp;
using MimeKit;

namespace IS_Turizmas.Controllers
{
    public class UserController : HomeController
    {

        private readonly ApplicationDbContext _context;
        private readonly SignInManager<RegistruotiVartotojai> _signInManager;
        private readonly RoleManager<VartotojoPlanai> _roleManager;
        private readonly IWebHostEnvironment _env;

        public UserController(ApplicationDbContext context, SignInManager<RegistruotiVartotojai> signInManager,
            RoleManager<VartotojoPlanai> roleManager, IWebHostEnvironment env):base(context)
        {
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _env = env;
        }


        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                var userId = _signInManager.UserManager.GetUserId(User);
                if (userId == null)
                {
                    return LocalRedirect("/");
                }

                id = int.Parse(userId);
            }
            else
            {
                if (_signInManager.IsSignedIn(User))
                {
                    var userId = int.Parse(_signInManager.UserManager.GetUserId(User));
                    var prenumerata = _context.Prenumeratos.Where(o => o.FkPrenumeruojamasis == id && o.FkPrenumeratorius == userId).FirstOrDefault();
                    if (prenumerata == null)
                    {
                        ViewBag.IsSubscribed = false;
                    }
                    else
                    {
                        ViewBag.IsSubscribed = true;
                    }


                }
                else
                {
                    ViewBag.IsSubscribed = false;
                }

            }

            RegistruotiVartotojai user = await _context.RegistruotiVartotojai.Include(o => o.VartotojoPlanai).ThenInclude(o => o.TipasNavigation).FirstOrDefaultAsync(o => o.Id == id);

            ViewBag.activityLevel = _context.AktyvumoLygiai.Where(o => o.Nuo <= user.AktyvumoTaskai && o.Iki >= user.AktyvumoTaskai).FirstOrDefault();

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
                var result = await _signInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    RegistruotiVartotojai user = _context.RegistruotiVartotojai.Where(o => o.Slapyvardis == username).FirstOrDefault();
                    if (user.PrisijungimoData.Date != DateTime.Now.Date)
                    {
                        AddActivityPoints(5, user);
                        TempData["AddedPoints"] = "Gavote 5 aktyvumo balus";
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
                    _context.VartotojoPlanai.Add(plan);
                    _context.SaveChangesAsync();
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    foreach (var error in result.Errors)
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
            " GimimoData")] EditViewRegistruotiVartotojai user, IFormFile naujaNuotrauka, string Slaptazodis, string oldPassword, string passwordConfirmation)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (naujaNuotrauka != null)
            {
                if (!IsImage(naujaNuotrauka))
                {
                    ModelState.AddModelError("", "Paveikslėlis turi būti paveikslo tipo");
                    return View("~/Views/User/EditUser.cshtml");
                }
            }

            Console.WriteLine("1");

            RegistruotiVartotojai currentUser = _context.RegistruotiVartotojai.Find(id);

            currentUser.Vardas = user.Vardas;
            currentUser.Pavarde = user.Pavarde;
            currentUser.ElPastas = user.ElPastas;
            currentUser.GimimoData = user.GimimoData;

            if (naujaNuotrauka != null)
            {
                if (currentUser.Nuotrauka != null)
                {
                    var path = Path.Combine(_env.WebRootPath, currentUser.Nuotrauka);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }


                string filename = currentUser.Slapyvardis + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + Path.GetExtension(naujaNuotrauka.FileName);
                var saveimg = Path.Combine(_env.WebRootPath, "images", "Users", filename);
                var stream = new FileStream(saveimg, FileMode.Create);
                await naujaNuotrauka.CopyToAsync(stream);

                currentUser.Nuotrauka = "images/Users/" + filename;
            }


            if (!ModelState.IsValid)
            {
                return View("~/Views/User/EditUser.cshtml");
            }

            Console.WriteLine("2");

            if (!(Slaptazodis == null && oldPassword == null && passwordConfirmation == null))
            {
                Console.WriteLine("3");

                if (Slaptazodis == null || oldPassword == null || passwordConfirmation == null)
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
                await _signInManager.SignOutAsync();
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

                if (versloVartotojas != null)
                {
                    var versloReklamos = _context.Reklamos.Where(o => o.FkVersloVartotojas == id).ToList<Reklamos>();
                    foreach (var reklama in versloReklamos)
                    {
                        var reklamosPlanai = _context.ReklamosPlanai.Where(o => o.FkReklama == reklama.Id).ToList<ReklamosPlanai>();
                        foreach (var planas in reklamosPlanai)
                        {
                            //Istrinami atitinkamos reklamos plano laikai.
                            _context.ReklamavimoLaikai.RemoveRange(_context.ReklamavimoLaikai.Where(o => o.FkReklamosPlanas == planas.Id));
                        }

                        //Istrinami atitinkami reklamos planai.
                        _context.ReklamosPlanai.RemoveRange(reklamosPlanai);
                    }

                    _context.Reklamos.RemoveRange(versloReklamos);
                    _context.VersloVartotojai.Remove(versloVartotojas);
                }

                //Vartotojo marsrutai
                var marsrutai = _context.Marsrutai.Where(o => o.FkRegistruotasVartotojas == id).ToList<Marsrutai>();
                foreach (var marsrutas in marsrutai)
                {
                    _context.MarsrutoObjektai.RemoveRange(_context.MarsrutoObjektai.Where(o => o.FkMarsrutas == marsrutas.Id));
                }

                _context.Marsrutai.RemoveRange(marsrutai);
                RemoveAllInnactiveVisitableObjects();
                RemoveAllInnactiveCountries();

                //Istrinamas registruotas vartotojas
                _context.RegistruotiVartotojai.Remove(_context.RegistruotiVartotojai.Find(id));
                _context.SaveChanges();

                TempData["DeletedMessage"] = "Profilis pašalintas";
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
                if(User.IsInRole("Verslo"))
                {
                    var versloVartotojas = _context.VersloVartotojai.FirstOrDefault(o => o.FkRegistruotasVartotojas == int.Parse(_signInManager.UserManager.GetUserId(User)));
                    return View(versloVartotojas);
                }
                return View();
            }
            else
            {
                return LocalRedirect("/");
            }
        }


        public async Task<IActionResult> ChangeSimplePlan(int id)
        {
            if (!_signInManager.IsSignedIn(User))
            {
                return LocalRedirect("/");
            }

            var userId = int.Parse(_signInManager.UserManager.GetUserId(User));

            if (id == 1)
            {
                if (User.IsInRole("Nemokamas"))
                {
                    return RedirectToAction("Index");
                }


                if (User.IsInRole("Verslo"))
                {
                    //Trinami verslo vartotojai
                    var versloVartotojas = _context.VersloVartotojai.FirstOrDefault(o => o.FkRegistruotasVartotojas == userId);

                    var versloReklamos = _context.Reklamos.Where(o => o.FkVersloVartotojas == userId).ToList<Reklamos>();
                    foreach (var reklama in versloReklamos)
                    {
                        var reklamosPlanai = _context.ReklamosPlanai.Where(o => o.FkReklama == reklama.Id).ToList<ReklamosPlanai>();
                        foreach (var planas in reklamosPlanai)
                        {
                            //Istrinami atitinkamos reklamos plano laikai.
                            _context.ReklamavimoLaikai.RemoveRange(_context.ReklamavimoLaikai.Where(o => o.FkReklamosPlanas == planas.Id));
                        }

                        //Istrinami atitinkami reklamos planai.
                        _context.ReklamosPlanai.RemoveRange(reklamosPlanai);
                    }
                }


                VartotojoPlanai senasPlanas = _context.VartotojoPlanai.Where(o => o.DataIki == null && o.FkRegistruotasVartotojas == userId).FirstOrDefault();
                senasPlanas.DataIki = DateTime.Now;
                _context.Update(senasPlanas);

                VartotojoPlanai naujasPlanas = new VartotojoPlanai();
                naujasPlanas.DataNuo = DateTime.Now;
                naujasPlanas.Tipas = id;
                naujasPlanas.FkRegistruotasVartotojas = userId;

                _context.VartotojoPlanai.Add(naujasPlanas);
                _context.SaveChangesAsync();
            }
            else if (id == 2)
            {
                if (User.IsInRole("Premium"))
                {
                    return RedirectToAction("Index");
                }


                if (User.IsInRole("Verslo"))
                {
                    //Trinami verslo vartotojai
                    var versloVartotojas = _context.VersloVartotojai.FirstOrDefault(o => o.FkRegistruotasVartotojas == userId);

                    var versloReklamos = _context.Reklamos.Where(o => o.FkVersloVartotojas == userId).ToList<Reklamos>();
                    foreach (var reklama in versloReklamos)
                    {
                        var reklamosPlanai = _context.ReklamosPlanai.Where(o => o.FkReklama == reklama.Id).ToList<ReklamosPlanai>();
                        foreach (var planas in reklamosPlanai)
                        {
                            //Istrinami atitinkamos reklamos plano laikai.
                            _context.ReklamavimoLaikai.RemoveRange(_context.ReklamavimoLaikai.Where(o => o.FkReklamosPlanas == planas.Id));
                        }

                        //Istrinami atitinkami reklamos planai.
                        _context.ReklamosPlanai.RemoveRange(reklamosPlanai);
                    }

                    _context.VersloVartotojai.Remove(versloVartotojas);
                }


                VartotojoPlanai senasPlanas = _context.VartotojoPlanai.Where(o => o.DataIki == null && o.FkRegistruotasVartotojas == userId).FirstOrDefault();
                senasPlanas.DataIki = DateTime.Now;
                _context.Update(senasPlanas);

                VartotojoPlanai naujasPlanas = new VartotojoPlanai();
                naujasPlanas.DataNuo = DateTime.Now;
                naujasPlanas.Tipas = id;
                naujasPlanas.FkRegistruotasVartotojas = userId;

                _context.VartotojoPlanai.Add(naujasPlanas);
                _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Profilio planas atnaujintas";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserPlan([Bind ("Imone, PastoKodas, Svetaine, Adresas")]  VersloVartotojai vartotojas)
        {
            var userId = int.Parse(_signInManager.UserManager.GetUserId(User));

            if (!ModelState.IsValid)
            {
                return View("~/Views/User/ChangePlan.cshtml");
            }

            if (User.IsInRole("Verslo"))
            {

                VersloVartotojai senasVartotojas = _context.VersloVartotojai.Where(o => o.FkRegistruotasVartotojas == userId).FirstOrDefault();
                senasVartotojas.Imone = vartotojas.Imone;
                senasVartotojas.PastoKodas = vartotojas.PastoKodas;
                senasVartotojas.Adresas = vartotojas.Adresas;
                senasVartotojas.Svetaine= vartotojas.Svetaine;

                _context.VersloVartotojai.Update(senasVartotojas);
                _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Verslo duomenys atnaujinti";

                return RedirectToAction("Index");
            }
            else
            {
                VartotojoPlanai senasPlanas = _context.VartotojoPlanai.Where(o => o.DataIki == null && o.FkRegistruotasVartotojas == userId).FirstOrDefault();
                senasPlanas.DataIki = DateTime.Now;
                _context.Update(senasPlanas);

                VartotojoPlanai naujasPlanas = new VartotojoPlanai();
                naujasPlanas.DataNuo = DateTime.Now;
                naujasPlanas.Tipas = 3;
                naujasPlanas.FkRegistruotasVartotojas = userId;

                _context.VartotojoPlanai.Add(naujasPlanas);

                vartotojas.FkRegistruotasVartotojas = userId;

                _context.VersloVartotojai.Add(vartotojas);
                _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Planas atnaujintas";

                return RedirectToAction("Index");
            }

            return LocalRedirect("/");
        }

        public void AddActivityPoints(int points, RegistruotiVartotojai user)
        {
            user.AktyvumoTaskai += points;
        }

        public async Task<IActionResult> Subscribe(int id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userId = int.Parse(_signInManager.UserManager.GetUserId(User));
                var prenumerata = _context.Prenumeratos.Where(o => o.FkPrenumeruojamasis == id && o.FkPrenumeratorius == userId).FirstOrDefault();
                if(prenumerata == null)
                {
                    Prenumeratos naujaPrenumerata = new Prenumeratos();
                    naujaPrenumerata.FkPrenumeratorius = userId;
                    naujaPrenumerata.FkPrenumeruojamasis = id;
                    naujaPrenumerata.Data = DateTime.Now;
                    _context.Add(naujaPrenumerata);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _context.Remove(prenumerata);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", new { id });
        }


        private bool IsImage(IFormFile postedFile)
        {
            const int ImageMinimumBytes = 512;

            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!postedFile.OpenReadStream().CanRead)
                {
                    return false;
                }
                //------------------------------------------
                //check whether the image size exceeding the limit or not
                //------------------------------------------ 
                if (postedFile.Length < ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new byte[ImageMinimumBytes];
                postedFile.OpenReadStream().Read(buffer, 0, ImageMinimumBytes);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }


            return true;
        }

        public void SendEmails()
        {
            var myJsonString = System.IO.File.ReadAllText("..\\config.json");
            var myJObject = JObject.Parse(myJsonString);

            var vartotojai = _context.RegistruotiVartotojai.ToList<RegistruotiVartotojai>();

            foreach(var vartotojas in vartotojai)
            {
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("Admin", "skanauslab@gmail.com");
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress(vartotojas.Slapyvardis,vartotojas.ElPastas);
                message.To.Add(to);

                message.Subject = "Pasiūlymų pranešimas";

                var text = "Tai Makaliaus pasiūlymų pranešimas!\n";

                List<Prenumeratos> prenumeratos = _context.Prenumeratos.Where(o => o.FkPrenumeratorius == vartotojas.Id && o.FkPrenumeruojamasisNavigation.Marsrutai.Count > 0).ToList<Prenumeratos>();
                if(prenumeratos.Count != 0)
                {
                    Random rand = new Random();
                    int i = rand.Next(prenumeratos.Count);

                    var prenumeratuMarsrutai = _context.Marsrutai.Where(o => o.FkRegistruotasVartotojas == prenumeratos[i].FkPrenumeruojamasis).OrderByDescending(o => o.SukurimoData).FirstOrDefault();

                    text += "Gal Jus sudomins šis Jūsų prenumeratų maršrutas:\n";
                    text += "https://localhost:44390/content/ViewRouteInfo/" + prenumeratuMarsrutai.Id.ToString() + "\n";
                }


                var marsrutai = _context.Marsrutai.Where(o => o.FkRegistruotasVartotojas != vartotojas.Id).OrderByDescending(o => o.SukurimoData).FirstOrDefault();

                text += "Gal Jus sudomins šis naujas maršrutas:\n";
                text += "https://localhost:44390/content/ViewRouteInfo/" + marsrutai.Id.ToString();

                PasiulymuPranesimai pasiulymas = new PasiulymuPranesimai();
                pasiulymas.Data = DateTime.Now;
                pasiulymas.FkRegistruotasVartotojas = vartotojas.Id;
                pasiulymas.Tekstas = text;
                _context.Add(pasiulymas);
                _context.SaveChanges();

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = text;

                message.Body = bodyBuilder.ToMessageBody();

                SmtpClient client = new SmtpClient();
                client.Connect(myJObject.SelectToken("EmailLogin[0].host").Value<string>(), myJObject.SelectToken("EmailLogin[0].port").Value<int>(), true);
                Console.WriteLine(myJObject.SelectToken("EmailLogin[0].host").Value<string>());
                client.Authenticate(myJObject.SelectToken("EmailLogin[0].Username").Value<string>(), myJObject.SelectToken("EmailLogin[0].Password").Value<string>());

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();
            }            
        }
    }
}
