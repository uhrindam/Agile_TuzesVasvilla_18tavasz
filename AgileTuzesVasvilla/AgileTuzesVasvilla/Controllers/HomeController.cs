using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AgileTuzesVasvilla.Controllers
{
    [Route("[Controller]/")]
    public class HomeController : Controller
    {
        [HttpGet("index")]
        public IActionResult Index()
        {
            return new JsonResult(new int[] { 42, 69, 125, 444 });
        }
    }
}
