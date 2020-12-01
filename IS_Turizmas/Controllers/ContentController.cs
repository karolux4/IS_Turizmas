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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace IS_Turizmas.Controllers
{
    public class ContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        //SqlCommand com = new SqlCommand();
        //SqlDataReader dr;
        //SqlConnection con = new SqlConnection();
        public ContentController(ApplicationDbContext context)
        {
            _context = context;
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

        public IActionResult FilterRoutes()
        {
            return View();
        }

        public IActionResult SearchRoutes()
        {
            return View();
        }

        public IActionResult FoundRoutes()
        {
            return View();
        }

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
