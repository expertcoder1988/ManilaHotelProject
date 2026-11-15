using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelWebSite.Controllers
{
    public class GalleryController : Controller
    {
        [Route("~/hotel-naran-pictures", Name = "galleryhotelnaran")]
        // GET: Gallery
        public ActionResult Index()
        {
            return View();
        }
    }
}