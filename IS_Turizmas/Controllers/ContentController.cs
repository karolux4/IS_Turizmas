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
            return View(await _context.Marsrutai.Include(o => o.FkRegistruotasVartotojasNavigation)
                .Include(b => b.Reitingai).Include(o => o.MarsrutoObjektai).ToListAsync());
        }


        private void FetchAllData()
        {

        }

        public async Task<IActionResult> ViewRouteInfo(int id)
        {
            //ViewBag.URL = "https://localhost:44390"+Request.Path;
            ViewBag.URL = "https://www.makalius.lt/";
            int test = id;
            ViewBag.route_points = _context.MarsrutoObjektai.Include(o => o.FkLankytinasObjektasNavigation)
                .Where(o => o.FkMarsrutas == id).OrderBy(o => o.EilesNr).Select(o => o.FkLankytinasObjektasNavigation.Pavadinimas).ToArray();
            ViewBag.comments = _context.Komentarai.Where(o => o.FkMarsrutas == id).Select(y => y.Tekstas).ToArray();
            //var routeObjects = _context.MarsrutoObjektai.Where(o => o.FkMarsrutas == id);
            //var routeObjNames = 
            //ViewBag.RouteObjects = _context.LankytiniObjektai.Where(o => o.MarsrutoObjektai. )
            //ViewBag.RouteObjects = _context.MarsrutoObjektai.Where(o => o.FkMarsrutas == id);
            return View(await _context.Marsrutai.Include(o => o.FkRegistruotasVartotojasNavigation)
                .Include(b => b.Reitingai).FirstOrDefaultAsync(o => o.Id == id));
        }



        public IActionResult ViewAllRouteAnalysis()
        {
            //int MaxUpToPrice = _context.Marsrutai.Max(o => o.IslaidosIki);
            //ViewBag.HighestUpToPricesRoutes = _context.Marsrutai.Where(ob => ob.IslaidosIki == MaxUpToPrice).ToList();

            int LowestPrice = _context.Marsrutai.Min(o => o.IslaidosNuo);
            ViewBag.LowestPricesRoutes = _context.Marsrutai.Where(ob => ob.IslaidosNuo == LowestPrice).ToList();

            var allCountries = _context.Valstybes.ToList();
            //int hasMostCountriesCount = 0;
            List<DataPoint> dataPoints = new List<DataPoint>();

            foreach (var item in allCountries)
            {
                var foundCount = _context.Marsrutai.Where(
                obj => obj.MarsrutoObjektai.Any(
                    b => b.FkLankytinasObjektasNavigation.FkValstybeNavigation.Id == item.Id)).Count();

                dataPoints.Add(new DataPoint(item.Pavadinimas, foundCount));

                //if (foundCount> hasMostCountriesCount)
                //{
                //    hasMostCountriesCount = foundCount;
                //    ViewBag.MostPopularCountry = item.Pavadinimas;
                //}
            }




            

            //dataPoints.Add(new DataPoint("USA", 121));
            //dataPoints.Add(new DataPoint("Great Britain", 67));
            //dataPoints.Add(new DataPoint("China", 70));
            //dataPoints.Add(new DataPoint("Russia", 56));
            //dataPoints.Add(new DataPoint("Germany", 42));
            //dataPoints.Add(new DataPoint("Japan", 41));
            //dataPoints.Add(new DataPoint("France", 42));
            //dataPoints.Add(new DataPoint("South Korea", 21));

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);




            //ViewBag.searchedRoutes = _context.Marsrutai.Where(obj => obj.MarsrutoObjektai.Any
            //(b => b.FkLankytinasObjektasNavigation.FkValstybeNavigation.Id == countryId)).ToList();
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
            ViewBag.searchedRoutes = _context.Marsrutai.Include(o => o.FkRegistruotasVartotojasNavigation).Include(b => b.Reitingai)
            .Where(obj => obj.MarsrutoObjektai.Any(b => b.FkLankytinasObjektasNavigation.FkValstybeNavigation.Id == countryId)).ToList();
            return View();
        }

        public async Task<IActionResult> searchRoutes()
        {
            ViewBag.Valstybes = _context.Valstybes.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FoundRoutes(string searchText, string filterCountry)
        {
            if (searchText == null)
            {
                TempData["ErrorMessage"] = "Paieškos žodis yra privalomas";
                return RedirectToAction("searchRoutes");
            }
            if (filterCountry == "Jokio")
            {
                ViewBag.searchedRoutes = _context.Marsrutai
            .Include(o => o.FkRegistruotasVartotojasNavigation).Include(b => b.Reitingai)
            .Where(obj => EF.Functions.Like(obj.Pavadinimas, "%" + searchText + "%")).ToList();
            }
            else
            {
                int countryId = Int32.Parse(filterCountry);
                ViewBag.searchedRoutes = _context.Marsrutai
            .Include(o => o.FkRegistruotasVartotojasNavigation).Include(b => b.Reitingai)
            .Where(obj => EF.Functions.Like(obj.Pavadinimas, "%" + searchText + "%"))
            .Where(obj => obj.MarsrutoObjektai.Any(b => b.FkLankytinasObjektasNavigation.FkValstybeNavigation.Id == countryId)).ToList();
            }            
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
            int routeId = comment.FkMarsrutas;
            if (comment.Tekstas == null)
            {
                TempData["ErrorMessage"] = "Jūs neparašėte komentaro";
                return RedirectToAction("ViewRouteInfo", new { id = routeId });
            }

            int userId = Int32.Parse(_signInManager.UserManager.GetUserId(this.User));

            comment.Data = DateTime.Now;
            comment.FkRegistruotasVartotojas = userId;
                     


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
