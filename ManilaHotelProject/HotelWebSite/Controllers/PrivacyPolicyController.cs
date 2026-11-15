using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelWebSite.Controllers
{
    public class PrivacyPolicyController : Controller
    {
        [Route("~/privacy-policy", Name = "PrivacyPolicy")]
        // GET: PrivacyPolicy
        public ActionResult Index()
        {
            return View();
        }
    }
}