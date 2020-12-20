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
    public class AdvertsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<RegistruotiVartotojai> _signInManager;
        public AdvertsController(ApplicationDbContext context, SignInManager<RegistruotiVartotojai> signInManager)
        {
            _context = context;
            _signInManager = signInManager;

        }
        public IActionResult Confirm()
        {
            return View();
        }

        public IActionResult ConfirmDelete()
        {
            return View();
        }

        //Seems ok. Check needed
        public async Task<IActionResult> AdvertList()
        {
            var userId = _signInManager.UserManager.GetUserId(User);
            if (userId == null)
            {
                return LocalRedirect("/");
            }
            int id = int.Parse(userId);
            return View(await _context.Reklamos.Where(o => o.FkVersloVartotojas == id).ToListAsync());
        }
        //Not ok yet
        public IActionResult CreateAdvert()
        {
            //IsBuisinessUser(User) Needed
            if (_signInManager.IsSignedIn(User))
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
        public IActionResult DeleteAdvertPlan()
        {
            return View();
        }

        public async Task<IActionResult> ViewAdvertStatistics(int id)
        {
            //Clicks calculate elsewhere
            if (_signInManager.IsSignedIn(User) &&
                _context.Reklamos.FirstOrDefault(o => o.Id == id).FkVersloVartotojas == int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                return View(await _context.Reklamos.FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
        }
        //Check needed
        public async Task<IActionResult> EditAdvert(int id)
        {
            if (_signInManager.IsSignedIn(User) &&
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

        //Dont bind Paveikslelis probs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertCreate([Bind("Pavadinimas, Url")]
        Reklamos advert)
        {
            advert.Paspaudimai = 0;
            advert.FkVersloVartotojas = int.Parse(_signInManager.UserManager.GetUserId(User));

            //Toks saugojimo formatas? most likely
            //Nzn kaip ideti i folderi

            advert.Paveikslelis = "~/images/Adds/" + advert.Pavadinimas + advert.Id;
            
            // Papildomas failu tvarkymas. Ar reikia?
            // advert.Paveikslelis = "~/images/Adds/" + _signInManager.UserManager.GetUserId(User) + "/" + advert.Pavadinimas + advert,Id.toString();

            try
            {
                _context.Reklamos.Add(advert);
                //Ideti png cj cia

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

        //Check needed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertEdit(int id, [Bind("Id,Pavadinimas, Paveikslelis, Url")]
        Reklamos advert)
        {
            if (id != advert.Id)
            {
                return NotFound();
            }
            Reklamos oldAdvert = _context.Reklamos.Find(id);
            if (oldAdvert.FkVersloVartotojas != int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                return NotFound();
            }
            oldAdvert.Pavadinimas = advert.Pavadinimas;
            oldAdvert.Url = advert.Url;
            oldAdvert.Paveikslelis = advert.Paveikslelis;
            if (!ModelState.IsValid)
            {
                return View("~/Views/Adverts/EditAdvert.cshtml");
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
    }
}
