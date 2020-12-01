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
    public class RoutesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoutesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Marsrutai.Include(o => o.MarsrutoObjektai).ToListAsync());
        }

        public IActionResult CreateRouteDescription()
        {
            return View();
        }

        public IActionResult EditRouteDescription()
        {
            return View();
        }

        public IActionResult AddRouteObjects()
        {
            return View();
        }

        public async Task<IActionResult> EditRouteObjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route_points = await _context.MarsrutoObjektai.Include(o => o.FkLankytinasObjektasNavigation).
                Include(o => o.FkMarsrutasNavigation).Where(o => o.FkMarsrutas == id).OrderBy(o => o.EilesNr).ToListAsync();


            return View(route_points);
        }

        public async Task<IActionResult> ViewMap(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route_points = await _context.MarsrutoObjektai.Include(o => o.FkLankytinasObjektasNavigation).
                Where(o=> o.FkMarsrutas==id).OrderBy(o => o.EilesNr).ToListAsync();

            var myJsonString = System.IO.File.ReadAllText("..\\config.json");
            var myJObject = JObject.Parse(myJsonString);
            var key = myJObject.SelectToken("MapEmbeded").Value<string>();

            var url = "https://www.google.com/maps/embed/v1/directions?key=" + key + "&";

            int last = route_points.Count-1;

            url += "origin=" + route_points[0].FkLankytinasObjektasNavigation.XKoordinate + "," +route_points[0].FkLankytinasObjektasNavigation.YKoordinate;
            url += "&destination=" + route_points[last].FkLankytinasObjektasNavigation.XKoordinate + "," + route_points[last].FkLankytinasObjektasNavigation.YKoordinate;

            string start = "&waypoints=";
            for (var i=1;i<last;i++)
            {
                url += start + route_points[i].FkLankytinasObjektasNavigation.XKoordinate + "," +
                    route_points[i].FkLankytinasObjektasNavigation.YKoordinate;
                start = "|";
            }
            url += "&avoid=tolls|highways";

            ViewBag.MapLink = url;

            return View(route_points);
        }

        public IActionResult CalculateRouteUniqueness()
        {
            return View();
        }

        //public IActionResult ViewAllRoutes()
        //{
        //    return View();
        //}

        //public IActionResult ViewRouteInfo()
        //{
        //    return View();
        //}

        //public IActionResult ViewAllRouteAnalysis()
        //{
        //    return View();
        //}

        //public IActionResult FilterRoutes()
        //{
        //    return View();
        //}

        //public IActionResult SearchRoutes()
        //{
        //    return View();
        //}

        //public IActionResult FoundRoutes()
        //{
        //    return View();
        //}

        //public IActionResult ShareRoute()
        //{
        //    return View();
        //}

        //public IActionResult Rate()
        //{
        //    return View();
        //}


        //public IActionResult Comment()
        //{
        //    return View();
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDescription()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDescription()
        {
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddObjects(string address)
        {
            /*HttpClient client = new HttpClient();

            string line = "address:Kaunas&key=AIzaSyDL5U4rZ7pfwLxxlRWy85rXflMJ93TC5mI";
            string encoded = HttpUtility.UrlEncode(line, Encoding.UTF8);

            var myJsonString = System.IO.File.ReadAllText("..\\config.json");
            var myJObject = JObject.Parse(myJsonString);
            var key=myJObject.SelectToken("GeoCodingkey").Value<string>();

            string url = "https://maps.googleapis.com/maps/api/geocode/json?";
            url += HttpUtility.UrlPathEncode("address="+address);
            url += HttpUtility.UrlPathEncode("&key="+key);
            



            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),

            };

            
            var response = await client.SendAsync(request).ConfigureAwait(false);

            if(response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Blogai");
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine(responseBody);
            }*/
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditObjects(string address)
        {
            return RedirectToAction(nameof(Index));
        }

    }
}
