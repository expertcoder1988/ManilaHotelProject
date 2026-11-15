using HotelWebSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelWebsite.Controllers
{
    public class BlogsController : Controller
    {
        HotelDBEntities db = new HotelDBEntities();

        [Route("~/blog-manila-naran", Name = "bloghotelnaran")]
        // GET: Blogs
        public ActionResult Index()
        {
            var lst = (from c in db.Posts where c.IsPublished == true select c).OrderByDescending(c => c.PublishedDate).ToList();
            ViewBag.blogs = lst;

            return View();
        }


        [Route("~/post/{postId:int}/{slug?}", Name = "postDetailRoute")]
        public ActionResult Post(int postId, string slug)
        {
            var blog = (from c in db.Posts
                        where c.PostId == postId && c.IsPublished == true
                        select c).ToList();
            TempData["blog"] = blog;

            return View();
        }

        //[Route("~/post/{postId:int}/{slug}", Name = "postDetailRoute")]
        //public ActionResult Post(int PostId, string slug)
        //{
        //    var blog = (from c in db.Posts where c.PostId == PostId select c).ToList();
        //    TempData["blog"] = blog;

        //    return View();
        //}

        public ActionResult Image(string img)
        {
            var image = "C:\\inetpub\\wwwroot\\MountFeastBlog\\uploads\\post\\" + img;
            //string path = image.Path; // could be any absolute path anywhere on your server, for example c:\foo\bar.jpg
            string contentType = "image/jpg"; // image.ContentType; // for example image/jpg
            return File(image, contentType);
        }
        public List<Post> GetAllBlogs()
        {
            var lst = (from c in db.Posts
                       where c.IsPublished == true
                       select c).OrderByDescending(c => c.PublishedDate).ToList();
            return lst;

        }
    }
}