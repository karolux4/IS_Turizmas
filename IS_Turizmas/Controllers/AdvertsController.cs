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
    public class AdvertsController : Controller
    {
        public IActionResult Confirm()
        {
            return View();
        }

        public IActionResult ConfirmDelete()
        {
            return View();
        }

        public IActionResult AdvertList()
        {
            return View();
        }

        public IActionResult CreateAdvert()
        {
            return View();
        }
        
        public IActionResult CreateAdvertPlan()
        {
            return View();
        }
        public IActionResult DeleteAdvertPlan()
        {
            return View();
        }

        public IActionResult ViewAdvertStatistics()
        {
            return View();
        }

        public IActionResult EditAdvert()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmActivation()
        {
            return RedirectToAction(nameof(AdvertList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertCreate()
        {
            return RedirectToAction(nameof(AdvertList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertPlanCreate()
        {
            return RedirectToAction(nameof(Confirm));
        }

        public IActionResult AdvertPlanDelete()
        {
            return RedirectToAction(nameof(ConfirmDelete));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdvertEdit()
        {
            return RedirectToAction(nameof(AdvertList));
        }
    }
}
