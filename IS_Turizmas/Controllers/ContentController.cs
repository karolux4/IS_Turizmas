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
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<RegistruotiVartotojai> _signInManager;
        public ContentController(ApplicationDbContext context,
            SignInManager<RegistruotiVartotojai> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> ViewAllRoutes()
        {
            return View(await _context.Marsrutai.Include(o => o.MarsrutoObjektai).ToListAsync());
        }


        private void FetchAllData()
        {

        }

        public async Task<IActionResult> ViewRouteInfo(int id)
        {
            int test = id;
            return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
        }



        public IActionResult ViewAllRouteAnalysis()
        {
            return View();
        }

        public async Task<IActionResult> FilterRoutes()
        {
            ViewBag.Valstybes = _context.Valstybes.ToList();
            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FoundFilterRoutes(string filterCountry)
        {
            int countryId = Int32.Parse(filterCountry);
            //ViewBag.searchedRoutes = _context.Marsrutai.Where(obj => obj.MarsrutoObjektai.Any(b => b.FkLankytinasObjektasNavigation.FkValstybeNavigation.Pavadinimas == filterCountry)).ToList();
            ViewBag.searchedRoutes = _context.Marsrutai.Where(obj => obj.MarsrutoObjektai.Any
            (b => b.FkLankytinasObjektasNavigation.FkValstybeNavigation.Id == countryId)).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FoundRoutes(string searchText)
        {
            //return "From [HttpPost]Index: filter on " + searchText;
            //var searchedRoutes = await _context.Marsrutai.Where(obj => EF.Functions.Like(obj.Pavadinimas, "%" + searchText + "%")).ToListAsync();
            //return View();
            ViewBag.searchedRoutes = _context.Marsrutai.Where(obj => EF.Functions.Like(obj.Pavadinimas, "%" + searchText + "%")).ToList();
            return View();
            //return View(await _context.Marsrutai.Where(obj => EF.Functions.Like(obj.Pavadinimas, "%" + searchText + "%")).ToListAsync);
        }

        public IActionResult ShareRoute()
        {
            return View();
        }

        public async Task<IActionResult> Rate(int Id)
        {
            ViewBag.id = Id;

            bool auth = _signInManager.Context.User.Identity.IsAuthenticated;

            if (!auth)
            {
                TempData["ErrorMessage"] = "Kad suteiktumėte reitingą, turite prisijungti.";
                return RedirectToAction("ViewRouteInfo", new { id = Id });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveRating([Bind("Reitingas, FkMarsrutas")] Reitingai rating)
        {
            int userId = Int32.Parse(_signInManager.UserManager.GetUserId(this.User));

            rating.Data = DateTime.Now;
            rating.FkRegistruotasVartotojas = userId;
            int routeId = rating.FkMarsrutas;


            try
            {
                _context.Reitingai.Add(rating);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
                throw;
            }



            TempData["SuccessMessage"] = "Reitingas pateiktas";
            return RedirectToAction("ViewRouteInfo", new { id = routeId });
        }


        public async Task<IActionResult> Comment(int Id)
        {
            ViewBag.id = Id;

            bool auth = _signInManager.Context.User.Identity.IsAuthenticated;

            if (!auth)
            {
                TempData["ErrorMessage"] = "Kad rašytumėte komentarą, turite prisijungti.";
                return RedirectToAction("ViewRouteInfo", new { id = Id });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([Bind("Tekstas, FkMarsrutas")] Komentarai comment)
        {
            int userId = Int32.Parse(_signInManager.UserManager.GetUserId(this.User));

            comment.Data = DateTime.Now;
            comment.FkRegistruotasVartotojas = userId;
            int routeId = comment.FkMarsrutas;           


            try
            {
                _context.Komentarai.Add(comment);
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                return NotFound();
                throw;
            }

            

            TempData["SuccessMessage"] = "Komentaras sukurtas";
            return RedirectToAction("ViewRouteInfo", new { id = routeId } );
        }



    }
}
