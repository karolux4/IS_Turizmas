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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditRouteObjects(int id, string[] address)
        {
            if (!ModelState.IsValid)
            {
                return View(id);
            }
            List<JObject> convertedAdress = new List<JObject>();
            int index = 1;
            foreach(string add in address)
            {
                JObject obj = GetJsonAddress(add).Result;
                if (obj == null)
                {
                    ModelState.AddModelError(string.Empty, "Nepavyko konvertuoti "+index+" adreso");
                    return View(id);
                }
                convertedAdress.Add(obj);
                index++;
            }
            try
            {
                _context.MarsrutoObjektai.RemoveRange(_context.MarsrutoObjektai.Where(o => o.FkMarsrutas == id));
                _context.SaveChanges();
                index = 1;
                foreach(JObject json in convertedAdress)
                {
                    string formatted_address = json.SelectToken("results[0].formatted_address").Value<string>();
                    LankytiniObjektai foundObject = _context.LankytiniObjektai.FirstOrDefault(o => o.Pavadinimas.Equals(formatted_address, StringComparison.OrdinalIgnoreCase));
                    LankytiniObjektai obj;
                    if (foundObject == null)
                    {
                        JToken country = GetCountry(json);
                        string countryShort = country.SelectToken("short_name").Value<string>();
                        Valstybes foundCountry = _context.Valstybes.FirstOrDefault(o => o.Trumpinys.Equals(countryShort, StringComparison.OrdinalIgnoreCase));
                        if(foundCountry == null)
                        {
                            Valstybes newCountry = new Valstybes();
                            newCountry.Pavadinimas = country.SelectToken("long_name").Value<string>();
                            newCountry.Trumpinys = countryShort;
                            newCountry.Zemynas = GetContinent(countryShort);
                            _context.Valstybes.Add(newCountry);
                            _context.SaveChanges();
                            int fk_Country = newCountry.Id;

                            LankytiniObjektai newObject = new LankytiniObjektai();
                            newObject.Pavadinimas = formatted_address;
                            newObject.XKoordinate = json.SelectToken("results[0].geometry.location.lng").Value<double>();
                            newObject.YKoordinate = json.SelectToken("results[0].geometry.location.lat").Value<double>();
                            newObject.FkValstybe = fk_Country;
                            _context.LankytiniObjektai.Add(newObject);
                            _context.SaveChanges();
                            obj = newObject;

                        }
                        else
                        {
                            LankytiniObjektai newObject = new LankytiniObjektai();
                            newObject.Pavadinimas = formatted_address;
                            newObject.XKoordinate = json.SelectToken("results[0].geometry.location.lng").Value<double>();
                            newObject.YKoordinate = json.SelectToken("results[0].geometry.location.lat").Value<double>();
                            newObject.FkValstybe = foundCountry.Id;
                            _context.LankytiniObjektai.Add(newObject);
                            _context.SaveChanges();
                            obj = newObject;
                        }
                    }
                    else
                    {
                        obj = foundObject;
                    }
                    MarsrutoObjektai routeObject = new MarsrutoObjektai();
                    routeObject.EilesNr = index;
                    routeObject.FkLankytinasObjektas = obj.Id;
                    routeObject.FkMarsrutas = id;
                    _context.MarsrutoObjektai.Add(routeObject);
                    _context.SaveChanges();
                    index++;
                }
                RemoveAllInnactiveVisitableObjects();
                RemoveAllInnactiveCountries();
            }
            catch(DbUpdateConcurrencyException)
            {
                return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
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

        private JToken GetCountry(JObject json)
        {
            JToken address_components = json.SelectToken("results[0].address_components");
            foreach(JToken comp in address_components.Children())
            {
                if (comp.SelectToken("types").Value<JArray>()[0].ToString().Equals("country"))
                {
                    return comp;
                }

            }
            
            return null;
        }

        private string GetContinent(string countryShortName)
        {
            var myJsonString = System.IO.File.ReadAllText("..\\continents.json");
            var myJObject = JObject.Parse(myJsonString);
            var continent = myJObject.SelectToken(countryShortName).Value<string>();
            return continent;
        }

        private async Task<JObject> GetJsonAddress(string address)
        {
            HttpClient client = new HttpClient();

            var myJsonString = System.IO.File.ReadAllText("..\\config.json");
            var myJObject = JObject.Parse(myJsonString);
            var key=myJObject.SelectToken("GeoCodingkey").Value<string>();

            string url = "https://maps.googleapis.com/maps/api/geocode/json?";
            url += HttpUtility.UrlPathEncode("address="+address);
            url += HttpUtility.UrlPathEncode("&key="+key+"&language=lt");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),

            };


            var response = await client.SendAsync(request).ConfigureAwait(false);

            if(response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseJson = JObject.Parse(responseBody);
                return responseJson;
            }
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

    }
}
