using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace IS_Turizmas.Controllers
{
    public class ContentController : Controller
    {

        public IActionResult ViewAllRoutes()
        {
            return View();
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
