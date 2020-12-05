using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class RoutesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<RegistruotiVartotojai> _signInManager;

        public RoutesController(ApplicationDbContext context, SignInManager<RegistruotiVartotojai> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Marsrutai.Include(o => o.MarsrutoObjektai).ToListAsync());
        }

        public async Task<IActionResult> CreateRouteDescription()
        {
            ViewBag.LaikoIverciai = _context.LaikoIverciai.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDescription([Bind("Pavadinimas, Aprasymas, LaikoIvertis, IslaidosNuo, IslaidosIki")]
        Marsrutai route)
        {
            route.Perziuros = 0;
            route.SukurimoData = DateTime.Now;
            route.ModifikavimoData = DateTime.Now;
            //System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            //route.FkRegistruotasVartotojas = int.Parse(_signInManager.UserManager.GetUserId(User));
            route.FkRegistruotasVartotojas = 1;
            if (!ModelState.IsValid)
            {
                ViewBag.LaikoIverciai = _context.LaikoIverciai.ToList();
                return View("~/Views/Routes/CreateRouteDescription.cshtml");
            }
            try
            {
                _context.Marsrutai.Add(route);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditRouteDescription(int id)
        {
            ViewBag.LaikoIverciai = _context.LaikoIverciai.ToList();
            return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id==id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditDescription(int id, [Bind("Id,Pavadinimas, Aprasymas, LaikoIvertis," +
            " IslaidosNuo, IslaidosIki")] Marsrutai route)
        {
            if(id!= route.Id)
            {
                return NotFound();
            }
            Marsrutai oldRoute = _context.Marsrutai.Find(id);
            oldRoute.Pavadinimas = route.Pavadinimas;
            oldRoute.Aprasymas = route.Aprasymas;
            oldRoute.LaikoIvertis = route.LaikoIvertis;
            oldRoute.IslaidosNuo = route.IslaidosNuo;
            oldRoute.IslaidosIki = route.IslaidosIki;
            oldRoute.ModifikavimoData = DateTime.Now;
            if (!ModelState.IsValid)
            {
                ViewBag.LaikoIverciai = _context.LaikoIverciai.ToList();
                return View("~/Views/Routes/EditRouteDescription.cshtml");
            }
            try
            {
                _context.Update(oldRoute);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!_context.Marsrutai.Any(o => o.Id == route.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddRouteObjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddRouteObjects(int id, string[] address)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Address"] = address;
                return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
            }
            if (address.Length == 1)
            {
                if (address[0]==null)
                {
                    return RedirectToAction(nameof(Index));
                }

            }
            List<JObject> convertedAdress = new List<JObject>();
            int index = 1;
            foreach (string add in address)
            {
                JObject obj = GetJsonAddress(add).Result;
                if (obj == null)
                {
                    ModelState.AddModelError(string.Empty, "Nepavyko konvertuoti " + index + " adreso");
                    ViewData["Address"] = address;
                    return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
                }
                if (obj.SelectToken("status").Value<string>() == "ZERO_RESULTS")
                {
                    ModelState.AddModelError(string.Empty, "Nepavyko konvertuoti " + index + " adreso");
                    ViewData["Address"] = address;
                    return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
                }
                convertedAdress.Add(obj);
                index++;
            }
            try
            {
                index = 1;
                foreach (JObject json in convertedAdress)
                {
                    string formatted_address = json.SelectToken("results[0].formatted_address").Value<string>();
                    LankytiniObjektai foundObject = _context.LankytiniObjektai.FirstOrDefault(o => o.Pavadinimas.Equals(formatted_address, StringComparison.OrdinalIgnoreCase));
                    LankytiniObjektai obj;
                    if (foundObject == null)
                    {
                        JToken country = GetCountry(json);
                        string countryShort = country.SelectToken("short_name").Value<string>();
                        Valstybes foundCountry = _context.Valstybes.FirstOrDefault(o => o.Trumpinys.Equals(countryShort, StringComparison.OrdinalIgnoreCase));
                        if (foundCountry == null)
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
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditRouteObjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var route_points = _context.MarsrutoObjektai.Include(o => o.FkLankytinasObjektasNavigation)
                .Where(o => o.FkMarsrutas == id).OrderBy(o => o.EilesNr).Select(o=> o.FkLankytinasObjektasNavigation.Pavadinimas).ToArray();
            ViewData["Address"] = route_points;


            return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id==id));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditRouteObjects(int id, string[] address)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Address"] = address;
                return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
            }
            if (address.Length == 1)
            {
                if (address[0] == null)
                {
                    _context.MarsrutoObjektai.RemoveRange(_context.MarsrutoObjektai.Where(o => o.FkMarsrutas == id));
                    _context.SaveChanges();
                    RemoveAllInnactiveVisitableObjects();
                    RemoveAllInnactiveCountries();
                    return RedirectToAction(nameof(Index));
                }

            }
            List<JObject> convertedAdress = new List<JObject>();
            int index = 1;
            foreach(string add in address)
            {
                JObject obj = GetJsonAddress(add).Result;
                if (obj == null)
                {
                    ModelState.AddModelError(string.Empty, "Nepavyko konvertuoti "+index+" adreso");
                    ViewData["Address"] = address;
                    return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
                }
                if (obj.SelectToken("status").Value<string>() == "ZERO_RESULTS")
                {
                    ModelState.AddModelError(string.Empty, "Nepavyko konvertuoti " + index + " adreso");
                    ViewData["Address"] = address;
                    return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
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

            url += "origin=" + route_points[0].FkLankytinasObjektasNavigation.YKoordinate.ToString(CultureInfo.InvariantCulture)
                + "," +route_points[0].FkLankytinasObjektasNavigation.XKoordinate.ToString(CultureInfo.InvariantCulture);
            url += "&destination=" + route_points[last].FkLankytinasObjektasNavigation.YKoordinate.ToString(CultureInfo.InvariantCulture)
                + "," + route_points[last].FkLankytinasObjektasNavigation.XKoordinate.ToString(CultureInfo.InvariantCulture);

            string start = "&waypoints=";
            for (var i=1;i<last;i++)
            {
                url += start + route_points[i].FkLankytinasObjektasNavigation.YKoordinate.ToString(CultureInfo.InvariantCulture) + "," +
                    route_points[i].FkLankytinasObjektasNavigation.XKoordinate.ToString(CultureInfo.InvariantCulture);
                start = "|";
            }
            url += "&avoid=tolls|highways";

            ViewBag.MapLink = url;

            return View(route_points);
        }

        public async Task<IActionResult> DeleteRoute(int id)
        {
            //if (this.User != null && _signInManager.UserManager.GetUserId(this.User) == _context.Marsrutai.Find(id).FkRegistruotasVartotojas.ToString())
            //{
                _context.MarsrutoObjektai.RemoveRange(_context.MarsrutoObjektai.Where(o => o.FkMarsrutas == id));
                _context.Komentarai.RemoveRange(_context.Komentarai.Where(o => o.FkMarsrutas == id));
                _context.Reitingai.RemoveRange(_context.Reitingai.Where(o => o.FkMarsrutas == id));
                _context.Marsrutai.Remove(_context.Marsrutai.Find(id));
                _context.SaveChanges();
                RemoveAllInnactiveVisitableObjects();
                RemoveAllInnactiveCountries();
           // }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CalculateRouteUniqueness()
        {
            return View();
        }      

    }
}
