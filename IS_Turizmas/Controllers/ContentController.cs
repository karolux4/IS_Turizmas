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
        //SqlCommand com = new SqlCommand();
        //SqlDataReader dr;
        //SqlConnection con = new SqlConnection();
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

        //public IActionResult ViewAllRoutes()
        //{
        //    //var movie = new Movie() { Name = "Test" };
        //    //var test = "Testas";
        //    //return View();
        //    return View(_context.Marsrutai.Include(o => o.MarsrutoObjektai));
        //}
        private void FetchAllData()
        {

        }


        public IActionResult ViewRouteInfo()
        {
            return View();
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

        //public IActionResult FoundRoutes()
        //{
        //    return View();
        //}

        public IActionResult ShareRoute()
        {
            return View();
        }

        public IActionResult Rate()
        {
            return View();
        }


        public IActionResult Comment()
        {
            return View();
        }

        
    }
}
