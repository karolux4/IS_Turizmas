using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IS_Turizmas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IS_Turizmas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult MainIndex()
        {
            return View("~/Views/Home/Index.cshtml");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            // your code here
            Reklamos reklama = _context.Reklamos.FirstOrDefault();
            ViewBag.CurrentAdId = reklama.Id;
            ViewBag.CurrentAd = reklama.Paveikslelis;
        }

    }
}
