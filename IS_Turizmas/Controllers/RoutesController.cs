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
            var userId = _signInManager.UserManager.GetUserId(User);
            if (userId == null)
            {
                return LocalRedirect("/");
            }
            int id = int.Parse(userId);
            return View(await _context.Marsrutai.Include(o => o.MarsrutoObjektai).Where(o => o.FkRegistruotasVartotojas==id).ToListAsync());
        }

        public async Task<IActionResult> CreateRouteDescription()
        {
            if (_signInManager.IsSignedIn(User))
            {
                ViewBag.LaikoIverciai = _context.LaikoIverciai.ToList();
                return View();
            }
            else
            {
                return LocalRedirect("/");
            }
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
            route.FkRegistruotasVartotojas = int.Parse(_signInManager.UserManager.GetUserId(User));
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
            TempData["SuccessMessage"] = "Maršruto aprašymas sukurtas";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditRouteDescription(int id)
        {
            if (_signInManager.IsSignedIn(User)&& 
                _context.Marsrutai.FirstOrDefault(o => o.Id == id).FkRegistruotasVartotojas == int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                ViewBag.LaikoIverciai = _context.LaikoIverciai.ToList();
                return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
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
            if (oldRoute.FkRegistruotasVartotojas != int.Parse(_signInManager.UserManager.GetUserId(User))){
                return NotFound();
            }
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
            TempData["SuccessMessage"] = "Maršruto aprašymas atnaujintas";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddRouteObjects(int id)
        {
            if (_signInManager.IsSignedIn(User) &&
                _context.Marsrutai.FirstOrDefault(o => o.Id == id).FkRegistruotasVartotojas == int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
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
                    TempData["SuccessMessage"] = "Maršrutas išliko nepakeistas (objektai nebuvo nurodyti)";
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
            TempData["SuccessMessage"] = "Maršruto objektai pridėti";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditRouteObjects(int id)
        {
            if (_signInManager.IsSignedIn(User) &&
                _context.Marsrutai.FirstOrDefault(o => o.Id == id).FkRegistruotasVartotojas == int.Parse(_signInManager.UserManager.GetUserId(User)))
            {
                var route_points = _context.MarsrutoObjektai.Include(o => o.FkLankytinasObjektasNavigation)
                .Where(o => o.FkMarsrutas == id).OrderBy(o => o.EilesNr).Select(o => o.FkLankytinasObjektasNavigation.Pavadinimas).ToArray();
                ViewData["Address"] = route_points;


                return View(await _context.Marsrutai.FirstOrDefaultAsync(o => o.Id == id));
            }
            else
            {
                return LocalRedirect("/");
            }
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
                    TempData["SuccessMessage"] = "Maršruto objektai buvo pašalinti";
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
            TempData["SuccessMessage"] = "Maršruto objektai atnaujinti";
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
            if (_signInManager.IsSignedIn(User)&&_signInManager.UserManager.GetUserId(User) == _context.Marsrutai.Find(id).FkRegistruotasVartotojas.ToString())
            {
                _context.MarsrutoObjektai.RemoveRange(_context.MarsrutoObjektai.Where(o => o.FkMarsrutas == id));
                _context.Komentarai.RemoveRange(_context.Komentarai.Where(o => o.FkMarsrutas == id));
                _context.Reitingai.RemoveRange(_context.Reitingai.Where(o => o.FkMarsrutas == id));
                _context.Marsrutai.Remove(_context.Marsrutai.Find(id));
                _context.SaveChanges();
                RemoveAllInnactiveVisitableObjects();
                RemoveAllInnactiveCountries();

                TempData["SuccessMessage"] = "Maršrutas pašalintas";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return LocalRedirect("/");
            }
        }

        public async Task<IActionResult> CalculateRouteUniqueness()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return View(await _context.Marsrutai.Where(o => o.FkRegistruotasVartotojas.ToString()==_signInManager.UserManager.GetUserId(User)).ToListAsync());
            }
            else
            {
                return LocalRedirect("/");
            }
        }  
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculateUniquenessIndex(string route, string start, string end, string intChecked)
        {
            Console.WriteLine(start + " " + end);
            int routeID;
            bool isGood = Int32.TryParse(route, out routeID);
            bool intCheck = (intChecked == "on") ? true : false;
            if (!isGood)
            {
                if (intCheck)
                {
                    TempData["StartDate"] = start;
                    TempData["EndDate"] = end;
                }
                ModelState.AddModelError("", "Neteisingas maršruto ID");
                return View("~/Views/Routes/CalculateRouteUniqueness.cshtml",
                    await _context.Marsrutai.Where(o => o.FkRegistruotasVartotojas.ToString() == _signInManager.UserManager.GetUserId(User)).ToListAsync());
            }

            bool useDates = false;

            DateTime startDate, endDate;
            if (start != null && end!=null && intCheck)
            {
                bool isGoodDate1 = DateTime.TryParseExact(start, "yyyy-dd-mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out startDate);
                bool isGoodDate2 = DateTime.TryParseExact(end, "yyyy-dd-mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out endDate);
                if (!isGoodDate1||!isGoodDate2)
                {
                    TempData["StartDate"] = start;
                    TempData["EndDate"] = end;
                    TempData["RouteID"] = route;
                    ModelState.AddModelError("", "Neteisingas datos formatas");
                    return View("~/Views/Routes/CalculateRouteUniqueness.cshtml", 
                        await _context.Marsrutai.Where(o => o.FkRegistruotasVartotojas.ToString() == _signInManager.UserManager.GetUserId(User)).ToListAsync());
                }
                useDates = true;
            }
            else
            {
                startDate = DateTime.Now;
                endDate = DateTime.Now;
            }

            List<MarsrutoObjektai> route_objects = _context.MarsrutoObjektai.Include(o => o.FkLankytinasObjektasNavigation).Where(o => o.FkMarsrutas == routeID).OrderBy(o => o.EilesNr).ToList();
            if (route_objects.Count < 2)
            {
                return RedirectToAction(nameof(CalculateRouteUniqueness));
            }

            List<Rectangle> rectangles = new List<Rectangle>();
            for (int i=0; i < route_objects.Count-1; i++)
            {
                //Declare objects
                LankytiniObjektai firstObject = route_objects[i].FkLankytinasObjektasNavigation;
                LankytiniObjektai secondObject = route_objects[i+1].FkLankytinasObjektasNavigation;

                Rectangle r = GetRectangle(firstObject, secondObject);
                Pixelate(ref r);
                rectangles.Add(r);

            }


            List<Marsrutai> comparingData;
            if (useDates)
            {
                comparingData = _context.Marsrutai.Include(o => o.MarsrutoObjektai).ThenInclude(o => o.FkLankytinasObjektasNavigation).Where(o => o.MarsrutoObjektai.Count > 1 &&
                    o.SukurimoData>=startDate && o.SukurimoData <= endDate && o.Id!=routeID).ToList();
            }
            else
            {
                comparingData = _context.Marsrutai.Include(o => o.MarsrutoObjektai).ThenInclude(o => o.FkLankytinasObjektasNavigation).
                    Where(o => o.MarsrutoObjektai.Count > 1 && o.Id!=routeID).ToList();
            }

            double best_score = 0;

            foreach(Marsrutai m in comparingData)
            {
                List<Rectangle> comparing_rectangles = new List<Rectangle>();
                List<MarsrutoObjektai> object_list = m.MarsrutoObjektai.ToList();
                for (int i = 0; i < object_list.Count - 1; i++)
                {
                    //Declare objects
                    LankytiniObjektai firstObject = object_list[i].FkLankytinasObjektasNavigation;
                    LankytiniObjektai secondObject = object_list[i + 1].FkLankytinasObjektasNavigation;

                    Rectangle r = GetRectangle(firstObject, secondObject);
                    comparing_rectangles.Add(r);

                }

                double inR= 0;
                double totalR = 0;

                for(int i = 0; i < rectangles.Count; i++)
                {
                    foreach(Point p in rectangles[i].middlePoints)
                    {
                        for(int j = 0; j < comparing_rectangles.Count; j++)
                        {
                            if(isPointInRectangle(p, comparing_rectangles[j]))
                            {
                                inR++;
                                break;
                            }
                        }
                        totalR++;
                    }
                }

                double score = 0;
                if (totalR > 0)
                {
                    score = inR / totalR;
                }
                if (score > best_score)
                {
                    best_score = score;
                }
            }

            double index = (1 - best_score) * 100;
            TempData["UniquenessIndex"] = index.ToString("F2", CultureInfo.InvariantCulture);
            TempData["RouteCount"] = comparingData.Count;
            TempData["RouteID"] = route;
            if (intCheck)
            {
                TempData["StartDate"] = start;
                TempData["EndDate"] = end;
            }


            return RedirectToAction(nameof(CalculateRouteUniqueness));
        }

        private bool isPointInRectangle(Point p, Rectangle r)
        {
            double area1 = TriangleArea(r.p1, p, r.p4);
            double area2 = TriangleArea(r.p4, p, r.p3);
            double area3 = TriangleArea(r.p3, p, r.p2);
            double area4 = TriangleArea(p, r.p2, r.p1);
            double sum = area1 + area2 + area3 + area4;
            if (sum > RectangleArea(r))
            {
                return false;
            }
            return true;
        }

        private double TriangleArea(Point p1, Point p2, Point p3)
        {
            double area = Math.Abs((p2.x*p1.y-p1.x*p2.y)+(p3.x*p2.y-p2.x*p3.y)+(p1.x*p3.y-p3.x*p1.y))/2;
            return area;
        }

        private double RectangleArea(Rectangle r)
        {
            double area=TriangleArea(r.p1, r.p2, r.p3) + TriangleArea(r.p2, r.p3, r.p4);
            return area;
        }

        const double vertical1kmDegree = 0.009009;
        const double horizontal1kmDegree = 0.009009;

        double additionalAngle = Math.Atan(vertical1kmDegree / horizontal1kmDegree);

        private void Pixelate(ref Rectangle r)
        {
            double length = Math.Sqrt(Math.Pow(r.p3.x - r.p1.x, 2) + Math.Pow(r.p3.y - r.p1.y, 2))/0.009009;
            int rotationModifier = 1;
            Console.WriteLine("Length: " + length);
            r.middlePoints = new List<Point>();
            if (r.angle < 0)
            {
                rotationModifier = -1;
            }
            for (double i = 0.5; i < length; i+=1)
            {
                Point start_point = new Point();
                start_point.x = r.p1.x+i*horizontal1kmDegree*Math.Cos(r.angle);
                start_point.y = r.p1.y+i*vertical1kmDegree*Math.Sin(r.angle);
                for(double j=0.5; j< 10; j+=1)
                {
                    Point p = new Point();
                    p.x = start_point.x+(j * horizontal1kmDegree) * Math.Cos(rotationModifier*1.570797 + r.angle);
                    p.y = start_point.y +(j*vertical1kmDegree)*Math.Sin(rotationModifier*1.570797 + r.angle);

                    r.middlePoints.Add(p);
                }
            }
        }

        private Rectangle GetRectangle(LankytiniObjektai firstObject, LankytiniObjektai secondObject)
        {

            //Calculate angle with horizontal axis
            double angle = GetAngle(firstObject.XKoordinate, firstObject.YKoordinate, secondObject.XKoordinate, secondObject.YKoordinate);
            double rotationAngle1 = 1.570797 - Math.Abs(angle);
            double rotationAngle2 = 1.570797 + Math.Abs(angle);

            Rectangle r = new Rectangle();
            r.angle = angle;
            //Calculate four points of rectangle
            if (angle >= 0)
            {
                Point p1 = new Point();
                p1.x = firstObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle1 * -1);
                p1.y = firstObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle1 * -1);
                Point p2 = new Point();
                p2.x = firstObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle2);
                p2.y = firstObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle2);


                Point p3 = new Point();
                p3.x = secondObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle1 * -1);
                p3.y = secondObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle1 * -1);
                Point p4 = new Point();
                p4.x = secondObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle2);
                p4.y = secondObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle2);

                r.p1 = p1;
                r.p2 = p2;
                r.p3 = p3;
                r.p4 = p4;

                if (secondObject.XKoordinate < firstObject.XKoordinate)
                {
                    r.p1 = p3;
                    r.p2 = p4;
                    r.p3 = p1;
                    r.p4 = p2;
                }
            }
            else
            {
                Point p1 = new Point();
                p1.x = firstObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle1);
                p1.y = firstObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle1);
                Point p2 = new Point();
                p2.x = firstObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle2 * -1);
                p2.y = firstObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle2 * -1);


                Point p3 = new Point();
                p3.x = secondObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle1);
                p3.y = secondObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle1);
                Point p4 = new Point();
                p4.x = secondObject.XKoordinate + (5 * horizontal1kmDegree) * Math.Cos(rotationAngle2 * -1);
                p4.y = secondObject.YKoordinate + (5 * vertical1kmDegree) * Math.Sin(rotationAngle2 * -1);

                r.p1 = p1;
                r.p2 = p2;
                r.p3 = p3;
                r.p4 = p4;

                if (secondObject.XKoordinate < firstObject.XKoordinate) {
                    r.p1 = p3;
                    r.p2 = p4;
                    r.p3 = p1;
                    r.p4 = p2;
                }
            }
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            Console.WriteLine("Rectangle:");
            Console.WriteLine("1: (" + r.p1.x.ToString(nfi) + ", " + r.p1.y.ToString(nfi)+")");
            Console.WriteLine("2: (" + r.p2.x.ToString(nfi) + ", " + r.p2.y.ToString(nfi) + ")");
            Console.WriteLine("3: (" + r.p3.x.ToString(nfi) + ", " + r.p3.y.ToString(nfi) + ")");
            Console.WriteLine("4: (" + r.p4.x.ToString(nfi) + ", " + r.p4.y.ToString(nfi) + ")");
            Console.WriteLine("Objektas:");
            Console.WriteLine("First: " + firstObject.XKoordinate + " " + firstObject.YKoordinate);
            Console.WriteLine("Second: " + secondObject.XKoordinate + " " + secondObject.YKoordinate);

            return r;
        }

        struct Rectangle
        {
            public Point p1 { get; set; }
            public Point p2 { get; set; }
            public Point p3 { get; set; }
            public Point p4 { get; set; }

            public List<Point> middlePoints { get; set; } 

            public double angle { get; set; }
        }

        struct Point
        {
            public double x { get; set; }
            public double y { get; set; }
        }

        private double GetAngle(double x1, double y1, double x2, double y2)
        {
            if (x1 == x2)
            {
                return 1.570797; //90 degrees in radians
            }
            else if (y1 == y2)
            {
                return 0;
            }
            else
            {
                return Math.Atan((y2 - y1) / (x2 - x1));
            }
        }

    }
}
