using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelWebSite.Controllers
{
    public class PlacesandActivitiesController : Controller
    {
        [Route("~/visit-naran-places", Name = "placestovisitinnaran")]
        // GET: PopularActivities
        public ActionResult Index()
        {
            return View();
        }
    }
}