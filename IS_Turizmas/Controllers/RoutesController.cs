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
    public class RoutesController : Controller
    {
        public IActionResult Index()
        {
            return View();
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

        public IActionResult ViewMap()
        {
            var myJsonString = System.IO.File.ReadAllText("..\\config.json");
            var myJObject = JObject.Parse(myJsonString);
            var key = myJObject.SelectToken("MapEmbeded").Value<string>();

            var url = "https://www.google.com/maps/embed/v1/directions?key=" + key+"&";
            url += "origin=Kaunas,Lithuania&destination=Kaunas,Lithuania&waypoints=Bialystok,Poland|Warsaw,Poland&avoid=tolls|highways";

            ViewBag.MapLink = url;

            return View();
        }

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
            HttpClient client = new HttpClient();

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
            }
            return RedirectToAction(nameof(Index));
        }

       
    }
}
