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

namespace IS_Turizmas.Controllers
{
    public class AdvertsController : HomeController
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<RegistruotiVartotojai> _signInManager;
        private readonly IWebHostEnvironment _env;
        public AdvertsController(ApplicationDbContext context, SignInManager<RegistruotiVartotojai> signInManager,
            IWebHostEnvironment env):base(context)
        {
            _context = context;
            _signInManager = signInManager;
            _env = env;

        }

        //Is it good???
        public async Task<IActionResult> GetRandomAdvert()
        {
            var advert_plans = await _context.ReklamosPlanai.Include(o => o.ReklamavimoLaikai).Where(o=> o.Laikas_nuo < DateTime.Now && o.Laikas_iki > DateTime.Now).ToListAsync();
            int sum = 0;
            Random random = new Random();
            foreach (ReklamosPlanai rp in advert_plans)
            {
                sum = sum + (int)rp.Kaina;
            }
            int r = random.Next(0, sum);
            foreach (ReklamosPlanai rp in advert_plans)
            {
                sum = sum - (int)rp.Kaina;
                if (sum <= 0)
                {
                    return View(await _context.Reklamos.Where(o => o.Id == rp.FkReklama).ToListAsync());
                }
            }
            return LocalRedirect("/");
        }
        public IActionResult Confirm()
        {
            return View();
        }

        public IActionResult ConfirmDelete()
        {
            return View();
        }
        public async Task<IActionResult> AdvertList()
        {
            var userId = _signInManager.UserManager.GetUserId(User);
            if (userId == null || !User.IsInRole("Verslo"))
            {
                return LocalRedirect("/");
            }
            int id = int.Parse(userId);
            return View(await _context.Reklamos.Where(o => o.FkVersloVartotojas == id).ToListAsync());
        }
        public IActionResult CreateAdvert()
        {
            if (_signInManager.IsSignedIn(User) && User.IsInRole("Verslo"))
            {
                return View();
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        public IActionResult CreateAdvertPlan()
        {
            return View();
        }
        public async Task<IActionResult> DeleteAdvertPlan()
        {
            var userId = _signInManager.UserManager.GetUserId(User);
            if (userId == null || !User.IsInRole("Verslo"))
            {
                return LocalRedirect("/");
            }
            int id = int.Parse(userId);
            ViewBag.ReklamosPlanai = _context.ReklamosPlanai.ToList();
            ViewBag.ReklamavimoLaikai = _context.ReklamavimoLaikai.ToList();
            return View(await _context.Reklamos.Include(o => o.ReklamosPlanai).Where(o => o.FkVersloVartotojas == id).ToListAsync());
        }

        public async Task<IActionResult> ViewAdvertStatistics(int id)
        {
            if (_signInManager.IsSignedIn(User) && User.IsInRole("Verslo") &&
                _context.Reklamos.FirstOrDefault(o => o.Id == id).FkVersloVartotojas == int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                return View(await _context.Reklamos.FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        public async Task<IActionResult> EditAdvert(int id)
        {
            if (_signInManager.IsSignedIn(User) && User.IsInRole("Verslo") &&
                _context.Reklamos.FirstOrDefault(o => o.Id == id).FkVersloVartotojas == int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                return View(await _context.Reklamos.FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmActivation()
        {
            return RedirectToAction(nameof(AdvertList));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertCreate([Bind("Pavadinimas, Url")]
        Reklamos advert, IFormFile Paveikslelis)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Adverts/CreateAdvert.cshtml");
            }
            if (Paveikslelis == null)
            {
                ModelState.AddModelError("", "Paveikslėlis yra privalomas");
                return View("~/Views/Adverts/CreateAdvert.cshtml");
            }

            if (!IsImage(Paveikslelis))
            {
                ModelState.AddModelError("", "Paveikslėlis turi būti paveikslo tipo");
                return View("~/Views/Adverts/CreateAdvert.cshtml");
            }

            advert.Paspaudimai = 0;
            advert.FkVersloVartotojas = int.Parse(_signInManager.UserManager.GetUserId(User));

            string filename = advert.Pavadinimas + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + Path.GetExtension(Paveikslelis.FileName);
            var saveimg = Path.Combine(_env.WebRootPath, "images", "Adds", filename);
            var stream = new FileStream(saveimg, FileMode.Create);
            await Paveikslelis.CopyToAsync(stream);

            advert.Paveikslelis = "images/Adds/"+filename;
            
            try
            {
                _context.Reklamos.Add(advert);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Reklama sukurta";
            return RedirectToAction(nameof(AdvertList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertPlanCreate()
        {
            return RedirectToAction(nameof(Confirm));
        }

        public IActionResult AdvertPlanDelete()
        {
            return RedirectToAction(nameof(ConfirmDelete));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertEdit(int id, [Bind("Id,Pavadinimas, Url")]
        Reklamos advert, IFormFile naujasPaveikslas)
        {
            if (id != advert.Id)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View("~/Views/Adverts/EditAdvert.cshtml");
            }
            if (naujasPaveikslas != null)
            {
                if (!IsImage(naujasPaveikslas))
                {
                    ModelState.AddModelError("", "Paveikslėlis turi būti paveikslo tipo");
                    return View("~/Views/Adverts/EditAdvert.cshtml");
                }
            }
            Reklamos oldAdvert = _context.Reklamos.Find(id);
            if (oldAdvert.FkVersloVartotojas != int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                return NotFound();
            }
            oldAdvert.Pavadinimas = advert.Pavadinimas;
            oldAdvert.Url = advert.Url;
            if (naujasPaveikslas != null)
            {
                var path = Path.Combine(_env.WebRootPath, oldAdvert.Paveikslelis);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                string filename = advert.Pavadinimas + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + Path.GetExtension(naujasPaveikslas.FileName);
                var saveimg = Path.Combine(_env.WebRootPath, "images", "Adds", filename);
                var stream = new FileStream(saveimg, FileMode.Create);
                await naujasPaveikslas.CopyToAsync(stream);

                advert.Paveikslelis = "images/Adds/" + filename;
                oldAdvert.Paveikslelis = advert.Paveikslelis;
            }
            
            try
            {
                _context.Update(oldAdvert);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reklamos.Any(o => o.Id == advert.Id))
                {
                    return NotFound();
                }
                throw;
            }
            TempData["SuccessMessage"] = "Reklama atnaujinta";
            return RedirectToAction(nameof(AdvertList));
        }

        public IActionResult GoToAdLink(int id)
        {
            Reklamos reklama = this._context.Reklamos.Where(o=> o.Id==id).FirstOrDefault();
            reklama.Paspaudimai=reklama.Paspaudimai+1;
            _context.Update(reklama);
            _context.SaveChanges();
            return Redirect(reklama.Url);
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
    }
}
