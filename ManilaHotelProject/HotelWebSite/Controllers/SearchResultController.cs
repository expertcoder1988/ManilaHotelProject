using HotelWebSite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Globalization;

namespace HotelWebSite.Controllers
{
    public class SearchResultController : Controller
    {
        HotelDBEntities HDE = new HotelDBEntities();
        Booking bi = new Booking();

        // GET: SearchResult
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchResults(string CheckIn, string CheckOut, int Adults, int Children, int NoOfRooms)
        {
            try
            {
                //int PropertyID = 1;
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    string DateString = CheckIn;
                    IFormatProvider culture = new CultureInfo("en-US", true);
                    DateTime datestart = DateTime.ParseExact(DateString, "MM/dd/yyyy", culture);

                    string DateString2 = CheckOut;
                    IFormatProvider culture2 = new CultureInfo("en-US", true);
                    DateTime dateend = DateTime.ParseExact(DateString2, "MM/dd/yyyy", culture2);

                    // Calculate number of days
                    int NoOfDays = (dateend - datestart).Days;
                    ViewBag.NoOfDays = NoOfDays;

                    //string[] sDate = CheckIn.Split('-');
                    //string startDate = sDate[1] + "/" + sDate[0] + "/" + sDate[2];
                    int id = 1;

                    //sDate = CheckOut.Split('-');
                    //string endDate = sDate[1] + "/" + sDate[0] + "/" + sDate[2];

                    IEnumerable<PropertySearch> rowsAffected = db.Database.SqlQuery<PropertySearch>("exec sp_GetSearchResults {0}, {1}, {2}, {3}, {4}",
                         Convert.ToDateTime(datestart), Convert.ToDateTime(dateend), Adults, Children, NoOfRooms).ToList();


                    foreach (var itm in rowsAffected)
                    {
                        int v = VacantRooms(itm.RoomCode, itm.RoomCateCode, CheckIn);
                        int b = BlockedRooms(itm.RoomCode, itm.RoomCateCode, CheckOut);
                        itm.NoOfRooms = itm.NoOfRooms - v;
                        itm.NoOfRooms = itm.NoOfRooms - b;

                        if (itm.NoOfRooms < 0)
                        {
                            itm.NoOfRooms = 0;
                        }
                        itm.Id = id;
                        id++;
                    }

                    Session["results"] = rowsAffected;

                    RoomcatePics();

                    return View();
                    //return Json(rowsAffected, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //ViewBag.Error = ex.Message;
                return Json(ex.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        public int VacantRooms(int? roomCode, int? RoomCateCode, string startDate)
        {
            try
            {
                int vacantRooms = 0;
                //int TotalRooms = 0;
                DateTime _startDate = Convert.ToDateTime(startDate);

                // string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    //TotalRooms = db.RoomNumbers.Count(x => x.RoomCode == roomCode && x.Status == false);

                    vacantRooms = db.Bookings.Count(x => x.RoomCode == roomCode && x.CheckIn == _startDate);
                }

                return vacantRooms; // Content(vacantRooms.ToString());
            }
            catch (Exception ex)
            {
                return 0;// Content(ex.Message);
            }
        }

        public int BlockedRooms(int? roomCode, int? RoomCateCode, string startDate)
        {
            try
            {
                DateTime _startDate = Convert.ToDateTime(startDate);
                int blockedRooms = 0;
                var BlockedRooms = (from obj in HDE.BlockedRooms
                                    where obj.CateId == RoomCateCode && obj.StartDate <= _startDate && obj.EndDate > _startDate && obj.Status == true
                                    select new
                                    {
                                        RoomNum = obj.RoomNum
                                    }).FirstOrDefault();

                if (BlockedRooms == null)
                {
                    blockedRooms = 0;
                }
                else
                {
                    blockedRooms = Convert.ToInt32(BlockedRooms.RoomNum);
                }

                return blockedRooms; // Content(vacantRooms.ToString());
            }
            catch (Exception ex)
            {
                return 0;// Content(ex.Message);
            }
        }

        public ActionResult RoomDetails(int Id)
        {
            try
            {
                //if (Session["params"] != null)
                //{
                //    string Params = (string)Session["params"];
                //    string[] p = Params.Split('&');
                //    DateTime checkin = Convert.ToDateTime(p[0]);
                //}
                RoomDetail(Id);
                // RoomPrice(Id, checkin);
                RoomPictures(Id);
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public JsonResult RoomPics(int Roomcate)
        {
            try
            {
                //int Id = requestRoom.Id; // HttpContext.Request.Form["Id"];
                //string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.RoomPictures
                                where (a.RoomCode == Roomcate)
                                select new
                                {
                                    ImageName = a.Picture
                                }).ToList();

                    ;
                    return Json(room, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string RoomcatePics()
        {
            try
            {
                //int Id = requestRoom.Id; // HttpContext.Request.Form["Id"];
                //string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.RoomPictures
                                where (a.Active == true)
                                select new RoomImages
                                {
                                    Category = a.RoomCode,
                                    ImageName = a.Picture
                                }).ToList();

                    TempData["Images"] = room;
                }

                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string RoomDetail(int Id)
        {
            try
            {
                //int Id = requestRoom.Id; // HttpContext.Request.Form["Id"];
                //string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.Rooms
                                join b in db.RoomCategories
                                on a.RoomCategory equals b.Id
                                where (b.Id == Id)
                                select new RoomDetails
                                {
                                    Id = a.ID,
                                    RoomCode = a.RoomCode,
                                    RoomCategoryId = a.RoomCategory,
                                    RoomCategory = b.Category,
                                    RoomDescription = a.RoomDescription,
                                    PropertyId = a.propertyID,
                                    BedType = a.BedType,
                                    CheckIn = a.CheckIn,
                                    CheckOut = a.CheckOut,
                                    NoOfRooms = a.NoOfRooms,
                                    Capacity = a.Capacity,
                                    Adult = a.Adult,
                                    NoOfChild = a.NoOfChild,
                                    ChildAge = a.ChildAge,
                                    ShortCode = a.ShortCode,
                                    MinimumPrice = a.MinimumPrice,
                                    MinimumStay = a.MinimumStay,
                                    MaximumStay = a.MaximumStay,
                                    Size = a.Size,
                                    Features = a.Features
                                }).FirstOrDefault();

                    //  TempData["room"] = room;
                    ViewBag.room = room;
                }



                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string RoomPrice(int Id, DateTime checkin)
        {
            try
            {
                // int Id = requestRoom.Id;
                // DateTime checkin = requestRoom.Checkin;
                //  string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var roomPrice = (from a in db.RateSlabs
                                     where (a.RoomCategoryCode == Id && (a.StartDate < checkin && a.EndDate > checkin))
                                     select new
                                     {
                                         RoomPrice = a.ExtraAdultPrice
                                     }).FirstOrDefault();
                    TempData["roomPrice"] = roomPrice;
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public JsonResult RoomPolicy(int CancellationPolicy)
        {
            try
            {
                using (var db = new HotelDBEntities())
                {
                    var room = (from a in db.RatePlans
                                join b in db.CancellationPolicies
                                        on a.CancellationPolicy equals b.Id
                                where (b.Id == CancellationPolicy && a.Status == true)
                                select new
                                {
                                    RatePlanName = a.Name,
                                    CancellationPolicy = b.Name,
                                    FreeCancelBefore = b.PeriodForFreeCharge,
                                    Breakfst = a.Breakfast,
                                    ChargedForNoShow = b.ChargedForNoShow,
                                    GuestChargedForCancellation = b.GuestChargedForCancellation,
                                    HowlongFreeOfCharge = b.HowlongFreeOfCharge
                                }).ToList();

                    ;
                    return Json(room, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string RoomPictures(int Id)
        {
            try
            {
                // int Id = requestRoom.Id;
                // DateTime checkin = requestRoom.Checkin;
                //  string jsonString = "";
                // var responseBasicProfile = new ResponsePropertySearch();
                using (var db = new HotelDBEntities())
                {
                    var roomPictures = (from a in db.RoomPictures
                                        join b in db.Rooms
                                        on a.RoomCode equals b.RoomCategory
                                        where (b.RoomCategory == Id && a.Active == true)
                                        select new Pictures
                                        {
                                            Picture = a.Picture
                                        }).ToList();

                    TempData["roomPictures"] = roomPictures;
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public ActionResult PaymentError(string pp_ResponseCode, string pp_ResponseMessage)
        {
            try
            {
                TempData["pp_ResponseMessage"] = pp_ResponseMessage;
                TempData["pp_ResponseCode"] = pp_ResponseCode;
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public ActionResult Thankyou(string basket_id, string err_code, string err_msg)
        {
            try
            {
                string confirmationId = "";
                string pp_ResponseCode = err_code; //Request.Form["status_code"];
                string pp_ResponseMessage = err_msg; // Request.Form["status_msg"];

                if (Request.Form["basket_id"] != null)
                {
                    confirmationId = Request.Form["basket_id"];
                }
                else
                {
                    confirmationId = basket_id;
                }

                if (pp_ResponseCode == "00" || pp_ResponseCode == "000")
                {
                    var _booking = (from obj in HDE.Bookings
                                    join b in HDE.RoomCategories
                                     on obj.RoomCatCode equals b.Id
                                    where (obj.ConfirmationNo == confirmationId)
                                    select new FinalBooking
                                    {
                                        BookingID = obj.BookingID,
                                        PaxTitle = obj.PaxTitle,
                                        PaxFName = obj.PaxFName,
                                        PaxLName = obj.PaxLName,
                                        NoOfRooms = obj.NoOfRooms,
                                        RoomCode = obj.RoomCode,
                                        RoomCatCode = obj.RoomCatCode,
                                        RoomCategory = b.Category,
                                        RoomNum = obj.RoomNum,
                                        PaxEmail = obj.PaxEmail,
                                        PaxPhoneNumber = obj.PaxPhoneNumber,
                                        PaxDescription = obj.PaxDescription,
                                        Adults = obj.Adults,
                                        Children = obj.Children,
                                        CheckIn = obj.CheckIn,
                                        CheckOut = obj.CheckOut,
                                        RoomRate = obj.RoomRate,
                                        HotelCode = obj.HotelCode,
                                        ConfirmationNo = obj.ConfirmationNo,
                                        NoofDay = obj.NoofDay,
                                        TotalRoomRate = obj.TotalRoomRate
                                    }).ToList();

                    //RoomCates = RoomCates.Remove(0, 1);
                    //TempData["RoomCates"] = RoomCates;
                    //TempData["noOfDay"] = noOfDay;
                    Session["booking"] = _booking;
                    Session["confirmationId"] = confirmationId;

                    using (var db = new HotelDBEntities())
                    {
                        var myData = db.Bookings.Where(x => x.ConfirmationNo == confirmationId).ToList();
                        myData.ForEach(m => m.BookingStatus = "Booked");
                        db.SaveChanges();
                    }


                    TimeSpan t = Convert.ToDateTime(_booking[0].CheckOut) - Convert.ToDateTime(_booking[0].CheckIn);
                    int noOfDay = (int)t.TotalDays;
                    var category = _booking[0].RoomCategory;

                    try
                    {
                        StringBuilder htmlString = new StringBuilder();
                        htmlString.Append(@"<html>");
                        htmlString.Append("<body>");
                        htmlString.Append("<h3>Thanks</h3>");
                        htmlString.Append("<p> New Order Received.</p>");
                        htmlString.Append("Confirmation Num.:  <b>" + confirmationId + "</b><br />");
                        htmlString.Append("<table border='1'><tr>");
                        htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
                        htmlString.Append("<tr><td>" + _booking[0].PaxTitle + " " + _booking[0].PaxFName + " " + _booking[0].PaxLName + " </td><td> " + _booking[0].PaxPhoneNumber + "</td>");
                        htmlString.Append("<td>" + category + "</td> ");
                        htmlString.Append("<td>" + _booking[0].CheckIn + "</td> ");
                        htmlString.Append("<td>" + _booking[0].CheckOut + "</td>");
                        htmlString.Append("</tr>");
                        htmlString.Append("</table><br />");
                        htmlString.Append("<span><b>Adults:</b></span><span>" + _booking[0].Adults + "</span><br />");
                        htmlString.Append("<span><b>Children:</b></span><span>" + _booking[0].Children + "</span><br />");
                        htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + _booking[0].NoOfRooms + "</span><br />");
                        htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
                        //htmlString.Append("<span><b>Total Amount:</b></span><span>" + _booking[0].TotalAmount + "</span><br />");
                        htmlString.Append("</body>");
                        htmlString.Append("</html>");

                        MailMessage mm = new MailMessage();
                        mm.IsBodyHtml = true;
                        mm.To.Add("info@manilaheights.com");

                        mm.From = new MailAddress("info@manilaheights.com");
                        mm.Subject = "Order Confirmation: " + confirmationId;
                        mm.Body = htmlString.ToString();
                        SmtpClient smc = new SmtpClient();
                        smc.Host = "mail.manilaheights.com";
                        smc.Port = 587;
                        smc.UseDefaultCredentials = false;
                        smc.EnableSsl = false;
                        smc.Credentials = new System.Net.NetworkCredential("info@manilaheights.com", "838m!xcM5");
                        smc.DeliveryMethod = SmtpDeliveryMethod.Network;

                        smc.Send(mm);


                    }
                    catch (Exception ex)
                    {
                        TempData["error"] = ex.Message;
                    }

                    if (_booking[0].PaxEmail != "")
                    {
                        try
                        {
                            StringBuilder htmlString = new StringBuilder();
                            htmlString.Append(@"<html>");
                            htmlString.Append("<body>");
                            htmlString.Append("<h3>Thanks</h3>");
                            htmlString.Append("<p> New Order Received.</p>");
                            htmlString.Append("Confirmation Num.:  <b>" + confirmationId + "</b><br />");
                            htmlString.Append("<table border='1'><tr>");
                            htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
                            htmlString.Append("<tr><td>" + _booking[0].PaxTitle + " " + _booking[0].PaxFName + " " + _booking[0].PaxLName + " </td><td> " + _booking[0].PaxPhoneNumber + "</td>");
                            htmlString.Append("<td>" + category + "</td> ");
                            htmlString.Append("<td>" + _booking[0].CheckIn + "</td> ");
                            htmlString.Append("<td>" + _booking[0].CheckOut + "</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table><br />");
                            htmlString.Append("<span><b>Adults:</b></span><span>" + _booking[0].Adults + "</span><br />");
                            htmlString.Append("<span><b>Children:</b></span><span>" + _booking[0].Children + "</span><br />");
                            htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + _booking[0].NoOfRooms + "</span><br />");
                            htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
                            //htmlString.Append("<span><b>Total Amount:</b></span><span>" + _booking[0].TotalAmount + "</span><br />");
                            htmlString.Append("</body>");
                            htmlString.Append("</html>");

                            MailMessage mm = new MailMessage();
                            mm.IsBodyHtml = true;
                            mm.To.Add(_booking[0].PaxEmail);

                            mm.From = new MailAddress("info@manilaheights.com");
                            mm.Subject = "Order Confirmation: " + confirmationId;
                            mm.Body = htmlString.ToString();
                            SmtpClient smc = new SmtpClient();
                            smc.Host = "mail.manilaheights.com";
                            smc.Port = 587;
                            smc.UseDefaultCredentials = false;
                            smc.EnableSsl = false;
                            smc.Credentials = new System.Net.NetworkCredential("info@manilaheights.com", "838m!xcM5");
                            smc.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smc.Send(mm);


                        }
                        catch (Exception ex)
                        {
                            TempData["error"] = ex.Message;
                        }

                    }

                    return View();
                }
                else
                {
                    return RedirectToAction("PaymentError", "SearchResult", new { @pp_ResponseCode = pp_ResponseCode, @pp_ResponseMessage = pp_ResponseMessage });
                }



            }
            catch (Exception ex)
            {

                return Json(new { status = "error", message = ex.ToString(), JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult Thankyou(FormCollection frm)
        {
            try
            {

                string PaxTitle = Request.Form["PaxTitle"];
                string PaxFName = Request.Form["PaxFName"];
                string PaxLName = Request.Form["PaxLName"];
                string CheckIn = Convert.ToString(Request.Form["CheckIn"]);
                string CheckOut = Convert.ToString(Request.Form["CheckOut"]);
                int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
                string PaxPhoneNumber = Request.Form["PaxPhoneNumber"];
                string PaxEmail = Request.Form["PaxEmail"];
                string PaxDescription = Request.Form["PaxDescription"];
                int Adults = Convert.ToInt32(Request.Form["Adults"]);
                int Children = Convert.ToInt32(Request.Form["Children"]);
                int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
                int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
                int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
                string RoomCatCodes = Request.Form["RoomCatCode"];
                string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
                string RatePlanIDs = Request.Form["RatePlanIDs"];
                string RoomRates = Request.Form["RoomRates"];

                //string[] _CheckIn = CheckIn.Split('-');
                //string[] _CheckOut = CheckOut.Split('-');

                //CheckIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
                //CheckOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

                if (Adults == 0)
                {
                    Adults = 1;
                }

                SaveBooking(PaxTitle, PaxFName, PaxLName, Convert.ToDateTime(CheckIn), Convert.ToDateTime(CheckOut), RoomCatCode, PaxPhoneNumber, PaxEmail, PaxDescription, Adults,
                    Children, NoOfRooms, RoomRate, TotalAmount, RoomCatCodes, RoomCateRoomNums, RatePlanIDs, RoomRates);

                return View();
            }
            catch (Exception ex)
            {

                return Json(new { status = "error", message = ex.ToString() });
            }
        }

        public ActionResult Reservation(DateTime checkIn, DateTime checkOut, int Adults, int Children, int NoOfRooms, int RoomRate, int TotalAmount, string RoomCatCodes,
            string Quatntity, string RatePlanIDs, string RoomRates)
        {
            var db = new HotelDBEntities();
            var roomcategory = Convert.ToInt32(RoomCatCodes);
            ViewBag.RoomCategory = db.RoomPictures.Where(x => x.RoomCode == roomcategory).FirstOrDefault();
            ViewBag.Category = db.RoomCategories.Where(x => x.Id == roomcategory).FirstOrDefault();
            ViewBag.checkIn = checkIn;
            ViewBag.checkOut = checkOut;
            ViewBag.Adults = Adults;
            ViewBag.Children = Children;
            ViewBag.NoOfRooms = NoOfRooms;
            ViewBag.RoomRate = RoomRate;
            ViewBag.TotalAmount = TotalAmount;
            ViewBag.RoomCatCodes = RoomCatCodes;
            ViewBag.RoomCateRoomNums = Quatntity;
            ViewBag.RatePlanIDs = RatePlanIDs;
            ViewBag.RoomRates = RoomRates;

            return View();
        }

        [HttpPost]
        public ActionResult ReservationPost()
        {

            string checkIn = Convert.ToString(Request.Form["CheckIn"]);
            string checkOut = Convert.ToString(Request.Form["CheckOut"]);
            int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
            int Adults = Convert.ToInt32(Request.Form["Adults"]);
            int Children = Convert.ToInt32(Request.Form["Children"]);
            int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
            int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
            int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
            string RoomCatCodes = Request.Form["RoomCatCode"];
            string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
            string RatePlanIDs = Request.Form["RatePlanIDs"];
            string RoomRates = Request.Form["RoomRates"];

            //string[] _CheckIn = checkIn.Split('-');
            //string[] _CheckOut = checkOut.Split('-');

            //checkIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
            //checkOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

            string DateString = checkIn;
            IFormatProvider culture = new CultureInfo("en-US", true);
            DateTime datestart = DateTime.ParseExact(DateString, "MM/dd/yyyy", culture);

            string DateString2 = checkOut;
            IFormatProvider culture2 = new CultureInfo("en-US", true);
            DateTime dateend = DateTime.ParseExact(DateString2, "MM/dd/yyyy", culture2);

            //DateTime CheckIn = Convert.ToDateTime(checkIn);
            //DateTime CheckOut = Convert.ToDateTime(checkOut);
            return RedirectToAction("Reservation", "SearchResult", new { @checkIn = datestart, @checkOut = dateend, @Adults = Adults, @Children = Children, @NoOfRooms = NoOfRooms, @RoomRate = RoomRate, @TotalAmount = TotalAmount, @RoomCatCodes = RoomCatCodes, @RoomCateRoomNums = RoomCateRoomNums, @RatePlanIDs = RatePlanIDs, @RoomRates = RoomRates });
        }

        [HttpPost]
        public ActionResult ReservationPost2()
        {

            string checkIn = Convert.ToString(Request.Form["CheckIn"]);
            string checkOut = Convert.ToString(Request.Form["CheckOut"]);
            //  int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
            int Adults = Convert.ToInt32(Request.Form["Adults"]);
            int Children = Convert.ToInt32(Request.Form["Children"]);
            int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
            int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
            int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
            string RoomCatCodes = Request.Form["RoomCate"];
            string txtQuatntity = Request.Form["txtQuatntity"];
            string RatePlanIDs = Request.Form["RatePlanIDs"];
            string RoomRates = Request.Form["RoomRates"];

            //string[] _CheckIn = checkIn.Split('-');
            //string[] _CheckOut = checkOut.Split('-');

            //checkIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
            //checkOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

            string DateString = checkIn;
            IFormatProvider culture = new CultureInfo("en-US", true);
            DateTime datestart = DateTime.ParseExact(DateString, "MM/dd/yyyy", culture);

            string DateString2 = checkOut;
            IFormatProvider culture2 = new CultureInfo("en-US", true);
            DateTime dateend = DateTime.ParseExact(DateString2, "MM/dd/yyyy", culture2);

            //DateTime CheckIn = Convert.ToDateTime(checkIn);
            //DateTime CheckOut = Convert.ToDateTime(checkOut);
            return RedirectToAction("Reservation", "SearchResult", new { @checkIn = datestart, @checkOut = dateend, @Adults = Adults, @Children = Children, @NoOfRooms = NoOfRooms, @RoomRate = RoomRate, @TotalAmount = TotalAmount, @RoomCatCodes = RoomCatCodes, @Quatntity = txtQuatntity, @RatePlanIDs = RatePlanIDs, @RoomRates = RoomRates });
        }

        public string SaveBooking(string PaxTitle, string PaxFName, string PaxLName, DateTime CheckIn, DateTime CheckOut, int RoomCatCode,
            string PaxPhoneNumber, string PaxEmail, string PaxDescription, int Adults, int Children, int NoOfRooms, int RoomRate, int TotalAmount,
            string RoomCatCodes, string RoomCateRoomNums, string RatePlanIDs, string RoomRates)
        {
            try
            {
                using (var db = new HotelDBEntities())
                {
                    var rowsAffected = db.Bookings.Where(x => x.PaxFName == PaxFName && x.PaxLName == PaxLName &&
                                x.CheckIn == CheckIn).FirstOrDefault();

                    if (true == true)
                    {
                        int? bookingID = db.Bookings.DefaultIfEmpty().Max(p => p == null ? 0 : p.BookingID);
                        string confirmationId = "MHH-" + Convert.ToString(bookingID + 1);
                        int? PropertyId = db.Rooms.Where(p => p.RoomCategory == RoomCatCode).Select(p => p.propertyID).FirstOrDefault();
                        int? RoomCode = 0;
                        string RoomCate = "";
                        string RoomCates = "";
                        TimeSpan t = Convert.ToDateTime(CheckOut) - Convert.ToDateTime(CheckIn);
                        int noOfDay = (int)t.TotalDays;
                        string[] _RoomCatCodes;
                        string[] _RoomCateRoomNums;
                        string[] _RatePlanIDs;
                        string[] _RoomRates;

                        RoomRates = RoomRates.Remove(0, 1);
                        _RoomRates = RoomRates.Split(',');

                        RoomCatCodes = RoomCatCodes.Remove(0, 1);
                        _RoomCatCodes = RoomCatCodes.Split(',');

                        RatePlanIDs = RatePlanIDs.Remove(0, 1);
                        _RatePlanIDs = RatePlanIDs.Split(',');

                        RoomCateRoomNums = RoomCateRoomNums.Remove(0, 1);
                        _RoomCateRoomNums = RoomCateRoomNums.Split(',');

                        //foreach (var itm in _RoomCatCodes)
                        for (int itm = 0; itm < _RoomCatCodes.Length; itm++)
                        {
                            int CateCode = Convert.ToInt32(_RoomCatCodes[itm]);
                            RoomCode = db.Rooms.Where(p => p.RoomCategory == CateCode).Select(p => p.RoomCode).FirstOrDefault();
                            RoomCate = db.RoomCategories.Where(p => p.Id == CateCode).Select(p => p.Category).FirstOrDefault();
                            RoomCates = RoomCates + "," + RoomCate;

                            //foreach (var itm1 in _RoomCateRoomNums)
                            {
                                int NoofRooms = Convert.ToInt32(_RoomCateRoomNums[itm]);

                                for (int i = 1; i <= NoofRooms; i++)
                                {
                                    var bookedRooms = db.Bookings.Where(x => x.RoomCode == RoomCode && x.CheckIn == CheckIn).Select(p => p.RoomNum).ToList();
                                    var RoomNum = (db.RoomNumbers.Where(p => p.RoomCode == RoomCode).Select(p => p.RoomNumber1).ToList());
                                    //var roomnum = db.RoomNumbers.Where(p => p.RoomNumber1 == RoomNum).FirstOrDefault();

                                    var roomnum = RoomNum.Except(bookedRooms).FirstOrDefault();

                                    if (roomnum != null)
                                    {
                                        //roomnum.Booked = true;

                                        //db.Entry(roomnum).State = System.Data.EntityState.Modified;
                                        //db.SaveChanges();

                                        Booking booking = new Booking();

                                        booking.PaxTitle = PaxTitle;
                                        booking.PaxFName = PaxFName;
                                        booking.PaxLName = PaxLName;
                                        booking.NoOfRooms = 1;
                                        booking.RoomCode = RoomCode;
                                        booking.RoomCatCode = CateCode;
                                        booking.RoomNum = roomnum;
                                        booking.PaxEmail = PaxEmail;
                                        booking.PaxPhoneNumber = PaxPhoneNumber;
                                        booking.PaxDescription = PaxDescription;
                                        booking.Adults = Adults;
                                        booking.Children = Children;
                                        booking.CheckIn = CheckIn;
                                        booking.CheckOut = CheckOut;
                                        booking.RoomRate = Convert.ToInt32(_RoomRates[itm]);
                                        booking.TotalRoomRate = Convert.ToInt32(_RoomRates[itm]);
                                        booking.HotelCode = 1;
                                        booking.ConfirmationNo = confirmationId;
                                        booking.DateModified = DateTime.Now;
                                        booking.BookingStatus = "Booked";
                                        booking.NoofDay = noOfDay;
                                        booking.TotalRoomRate = TotalAmount;
                                        booking.ThemeColor = "red";
                                        booking.RatePlanId = Convert.ToInt32(_RatePlanIDs[itm]);

                                        db.Entry(booking).State = System.Data.Entity.EntityState.Added;
                                        db.SaveChanges();




                                    }
                                }
                            }
                        }

                        var _booking = (from obj in HDE.Bookings
                                        join b in HDE.RoomCategories
                                         on obj.RoomCatCode equals b.Id
                                        where (obj.ConfirmationNo == confirmationId)
                                        select new FinalBooking
                                        {
                                            BookingID = obj.BookingID,
                                            PaxTitle = obj.PaxTitle,
                                            PaxFName = obj.PaxFName,
                                            PaxLName = obj.PaxLName,
                                            NoOfRooms = obj.NoOfRooms,
                                            RoomCode = obj.RoomCode,
                                            RoomCatCode = obj.RoomCatCode,
                                            RoomCategory = b.Category,
                                            RoomNum = obj.RoomNum,
                                            PaxEmail = obj.PaxEmail,
                                            PaxPhoneNumber = obj.PaxPhoneNumber,
                                            PaxDescription = obj.PaxDescription,
                                            Adults = obj.Adults,
                                            Children = obj.Children,
                                            CheckIn = obj.CheckIn,
                                            CheckOut = obj.CheckOut,
                                            RoomRate = obj.RoomRate,
                                            HotelCode = obj.HotelCode,
                                            ConfirmationNo = obj.ConfirmationNo,
                                            NoofDay = obj.NoofDay,
                                            TotalRoomRate = obj.TotalRoomRate
                                        }).ToList();

                        RoomCates = RoomCates.Remove(0, 1);
                        TempData["RoomCates"] = RoomCates;
                        TempData["noOfDay"] = noOfDay;
                        Session["booking"] = _booking;
                        Session["confirmationId"] = confirmationId;

                        try
                        {
                            StringBuilder htmlString = new StringBuilder();
                            htmlString.Append(@"<html>");
                            htmlString.Append("<body>");
                            htmlString.Append("<h3>Thanks</h3>");
                            htmlString.Append("<p> New Order Received.</p>");
                            htmlString.Append("Confirmation Num.:  <b>" + confirmationId + "</b><br />");
                            htmlString.Append("<table border='1'><tr>");
                            htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
                            htmlString.Append("<tr><td>" + PaxTitle + " " + PaxFName + " " + PaxLName + " </td><td> " + PaxPhoneNumber + "</td>");
                            htmlString.Append("<td>" + RoomCates + "</td>");
                            htmlString.Append("<td>" + CheckIn + "</td> ");
                            htmlString.Append("<td>" + CheckOut + "</td>");
                            htmlString.Append("</tr>");
                            htmlString.Append("</table><br />");
                            htmlString.Append("<span><b>Adults:</b></span><span>" + Adults + "</span><br />");
                            htmlString.Append("<span><b>Children:</b></span><span>" + Children + "</span><br />");
                            htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + NoOfRooms + "</span><br />");
                            htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
                            htmlString.Append("<span><b>Total Amount:</b></span><span>" + TotalAmount + "</span><br />");
                            htmlString.Append("</body>");
                            htmlString.Append("</html>");

                            MailMessage mm = new MailMessage();
                            mm.IsBodyHtml = true;
                            mm.To.Add("info@manilaheights.com");

                            mm.From = new MailAddress("growbiztec@gmail.com");
                            mm.Subject = "Order Confirmation: " + confirmationId;
                            mm.Body = htmlString.ToString();
                            SmtpClient smc = new SmtpClient();
                            smc.Host = "smtp.gmail.com";
                            smc.Port = 587;
                            // smc.UseDefaultCredentials = false;
                            smc.EnableSsl = true;
                            smc.Credentials = new System.Net.NetworkCredential("growbiztec@gmail.com", "growbiz@2022");
                            smc.DeliveryMethod = SmtpDeliveryMethod.Network;

                            smc.Send(mm);


                        }
                        catch (Exception ex)
                        {
                            TempData["error"] = ex.Message;
                        }

                        if (PaxEmail != "")
                        {
                            try
                            {
                                StringBuilder htmlString = new StringBuilder();
                                htmlString.Append(@"<html>");
                                htmlString.Append("<body>");
                                htmlString.Append("<h3>Thanks</h3>");
                                htmlString.Append("<p> We have received your booking order and are working on a confirmation.</p>");
                                htmlString.Append("Your Confirmation Num.:  <b>" + confirmationId + "</b><br />");
                                htmlString.Append("<table border='1'><tr>");
                                htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
                                htmlString.Append("<tr><td>" + PaxTitle + " " + PaxFName + " " + PaxLName + " </td><td>" + PaxPhoneNumber + " </td>");
                                htmlString.Append("<td>" + RoomCates + "</td>");
                                htmlString.Append("<td>" + CheckIn + "</td> ");
                                htmlString.Append("<td>" + CheckOut + "</td>");
                                htmlString.Append("</tr>");
                                htmlString.Append("</table><br />");
                                htmlString.Append("<span><b>Adults:</b></span><span>" + Adults + "</span><br />");
                                htmlString.Append("<span><b>Children:</b></span><span>" + Children + "</span><br />");
                                htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + NoOfRooms + "</span><br />");
                                htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
                                htmlString.Append("<span><b>Total Amount:</b></span><span>" + TotalAmount + "</span><br />");
                                htmlString.Append("</body>");
                                htmlString.Append("</html>");

                                MailMessage mm = new MailMessage();
                                mm.IsBodyHtml = true;
                                mm.To.Add(PaxEmail);

                                mm.From = new MailAddress("info@manilaheights.com");
                                mm.Subject = "Order Confirmation: " + confirmationId;
                                mm.Body = htmlString.ToString();
                                SmtpClient smc = new SmtpClient();
                                smc.Host = "smtp.gmail.com";
                                smc.Port = 587;
                                // smc.UseDefaultCredentials = false;
                                smc.EnableSsl = true;
                                smc.Credentials = new System.Net.NetworkCredential("info@manilaheights.com", "Ne5h#2a38");
                                smc.DeliveryMethod = SmtpDeliveryMethod.Network;
                                smc.Send(mm);


                            }
                            catch (Exception ex)
                            {
                                TempData["error"] = ex.Message;
                            }

                        }

                        return ("Booking done succesfully");
                        //jsonString = JsonSerializer.Serialize(confirmationId);

                    }
                    else
                    {
                        // responseBasicProfile.MessageList.Add("Not Record Found.");
                        return ("Something went wrong.");
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return ("Something went wrong.");
            }
        }

        [HttpPost]
        public ActionResult SaveBooking(FormCollection frm)
        {
            try
            {
                string PaymentType = Request.Form["PaymentType"];
                string PaxTitle = Request.Form["PaxTitle"];
                string PaxFName = Request.Form["PaxFName"];
                string PaxLName = Request.Form["PaxLName"];
                string checkIn = Convert.ToString(Request.Form["CheckIn"]);
                string checkOut = Convert.ToString(Request.Form["CheckOut"]);
                int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
                string PaxPhoneNumber = Request.Form["PaxPhoneNumber"];
                string PaxEmail = Request.Form["PaxEmail"];
                string PaxDescription = Request.Form["PaxDescription"];
                int Adults = Convert.ToInt32(Request.Form["Adults"]);
                int Children = Convert.ToInt32(Request.Form["Children"]);
                int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
                int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
                int TotalAmount = Convert.ToInt32(Request.Form["TotalAmount"]);
                string RoomCatCodes = Request.Form["RoomCatCode"];
                string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
                string RatePlanIDs = Request.Form["RatePlanIDs"];
                string RoomRates = Request.Form["RoomRates"];
                int DiscountedAmount = Convert.ToInt32(Request.Form["DiscountedAmount"]);
                string VoucherCode = Request.Form["VoucherCode"];

                // string[] _CheckIn = checkIn.Split('-');
                // string[] _CheckOut = checkOut.Split('-');

                // checkIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
                // checkOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

                DateTime CheckIn = Convert.ToDateTime(checkIn);
                DateTime CheckOut = Convert.ToDateTime(checkOut);

                if (Adults == 0)
                {
                    Adults = 1;
                }

                using (var db = new HotelDBEntities())
                {
                    var rowsAffected = db.Bookings.Where(x => x.PaxFName == PaxFName && x.PaxLName == PaxLName &&
                                x.CheckIn == CheckIn).FirstOrDefault();

                    if (true == true)
                    {
                        int? bookingID = db.Bookings.DefaultIfEmpty().Max(p => p == null ? 0 : p.BookingID);
                        string confirmationId = "MHH-" + Convert.ToString(bookingID + 1);
                        int? PropertyId = db.Rooms.Where(p => p.RoomCategory == RoomCatCode).Select(p => p.propertyID).FirstOrDefault();
                        int? RoomCode = 0;
                        string RoomCate = "";
                        string RoomCates = "";
                        TimeSpan t = Convert.ToDateTime(CheckOut) - Convert.ToDateTime(CheckIn);
                        int noOfDay = (int)t.TotalDays;
                        //string[] _RoomCatCodes;
                        //string[] _RoomCateRoomNums;
                        //string[] _RatePlanIDs;
                        //string[] _RoomRates;
                        double GrandTotal = 0;

                        //RoomRates = RoomRates.Remove(0, 1);
                        //_RoomRates = RoomRates.Split(',');

                        //RoomCatCodes = RoomCatCodes.Remove(0, 1);
                        //_RoomCatCodes = RoomCatCodes.Split(',');

                        //RatePlanIDs = RatePlanIDs.Remove(0, 1);
                        //_RatePlanIDs = RatePlanIDs.Split(',');

                        //RoomCateRoomNums = RoomCateRoomNums.Remove(0, 1);
                        //_RoomCateRoomNums = RoomCateRoomNums.Split(',');

                        //foreach (var itm in _RoomCatCodes)
                        //for (int itm = 0; itm < RoomCatCodes.Length; itm++)
                        {
                            int CateCode = Convert.ToInt32(RoomCatCodes);
                            RoomCode = db.Rooms.Where(p => p.RoomCategory == CateCode).Select(p => p.RoomCode).FirstOrDefault();
                            RoomCate = db.RoomCategories.Where(p => p.Id == CateCode).Select(p => p.Category).FirstOrDefault();
                            RoomCates = RoomCates + "," + RoomCate;

                            //foreach (var itm1 in _RoomCateRoomNums)
                            {
                                int NoofRooms = Convert.ToInt32(RoomCateRoomNums);

                                //for (int i = 1; i <= NoofRooms; i++)
                                {
                                    var bookedRooms = db.Bookings.Where(x => x.RoomCode == RoomCode && x.CheckIn == CheckIn).Select(p => p.RoomNum).ToList();
                                    var RoomNum = (db.RoomNumbers.Where(p => p.RoomCode == RoomCode).Select(p => p.RoomNumber1).ToList());
                                    //var roomnum = db.RoomNumbers.Where(p => p.RoomNumber1 == RoomNum).FirstOrDefault();

                                    var roomnum = RoomNum.Except(bookedRooms).FirstOrDefault();

                                    if (roomnum != null)
                                    {
                                        //roomnum.Booked = true;

                                        //db.Entry(roomnum).State = System.Data.EntityState.Modified;
                                        //db.SaveChanges();

                                        GrandTotal = GrandTotal + TotalAmount;

                                        Booking booking = new Booking();

                                        booking.PaxTitle = PaxTitle;
                                        booking.PaxFName = PaxFName;
                                        booking.PaxLName = PaxLName;
                                        booking.NoOfRooms = 1;
                                        booking.RoomCode = RoomCode;
                                        booking.RoomCatCode = CateCode;
                                        booking.RoomNum = roomnum;
                                        booking.PaxEmail = PaxEmail;
                                        booking.PaxPhoneNumber = PaxPhoneNumber;
                                        booking.PaxDescription = PaxDescription;
                                        booking.Adults = Adults;
                                        booking.Children = Children;
                                        booking.CheckIn = CheckIn;
                                        booking.CheckOut = CheckOut;
                                      //  booking.RoomRate = Convert.ToInt32(RoomRates);
                                     //   booking.TotalRoomRate = Convert.ToInt32(RoomRates);
                                        booking.HotelCode = 1;
                                        booking.ConfirmationNo = confirmationId;
                                        booking.DateModified = DateTime.Now;
                                        booking.BookingStatus = "Pending";
                                        booking.NoofDay = noOfDay;
                                        //   booking.TotalRoomRate = TotalAmount;


                                        // Get the discount amount using the voucher code
                                        decimal discountAmount = GetDiscountAmountDecimal(VoucherCode);
                                        GrandTotal -= (double)discountAmount;
                                        DiscountedAmount -= (int)discountAmount;

                                        // Insert logic to set TotalRoomRate
                                        if (discountAmount > 0)
                                        {
                                            booking.TotalRoomRate = Convert.ToInt32(GrandTotal);
                                        }
                                        else
                                        {
                                            booking.TotalRoomRate = TotalAmount;
                                        }

                                        if (discountAmount > 0)
                                        {
                                            booking.RoomRate = Convert.ToInt32(GrandTotal);
                                        }
                                        else
                                        {
                                            booking.RoomRate = TotalAmount;
                                        }

                                        booking.ThemeColor = "red";
                                        booking.RatePlanId = Convert.ToInt32(RatePlanIDs);
                                        booking.ReservationDate = DateTime.Now;

                                        if (PaymentType == "1")
                                        {
                                            booking.DiscountedAmount = 0;
                                        }
                                        else
                                        {
                                            booking.DiscountedAmount = Convert.ToInt32(DiscountedAmount);
                                        }

                                        db.Entry(booking).State = System.Data.Entity.EntityState.Added;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        // Get the discount amount using the voucher code
                        decimal discAmount = GetDiscountAmountDecimal(VoucherCode);
                        DiscountedAmount -= (int)discAmount;

                        if (PaymentType == "1" || PaymentType == null)
                        {
                            return RedirectToAction("Thankyou", "SearchResult", new { @basket_id = confirmationId, @Total = GrandTotal, @err_code = "00", @err_msg = "" });
                        }
                        else
                        {
                            return RedirectToAction("GetAccessToken", "Payfast", new { @billReference = confirmationId, @GrandTotal = GrandTotal });
                        }

                    }
                    else
                    {
                        // responseBasicProfile.MessageList.Add("Not Record Found.");
                        return RedirectToAction("Index", "Home");

                        //return null;
                    }
                }

            }
            catch (Exception ex)
            {
                //TempData["error"] = ex.Message;
                return RedirectToAction("Index", "Home");
                // return null;
            }
        }

        public JsonResult GetDiscountAmount(string code)
        {
            var voucher = HDE.Vouchers.FirstOrDefault(v => v.Code == code);

            if (voucher != null)
            {
                return Json(new { success = true, discountAmount = voucher.DiscountAmount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, discountAmount = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        private decimal GetDiscountAmountDecimal(string code)
        {
            var voucher = HDE.Vouchers.FirstOrDefault(v => v.Code == code);
            return voucher?.DiscountAmount ?? 0;
        }
    }

    public class BookedRooms
    {
        public string RoomCate { get; set; }
        public int? Quantity { get; set; }
    }

    public class RoomImages
    {
        public int? Category { get; set; }
        public string ImageName { get; set; }
    }

    public class FinalBooking
    {
        public int? BookingID { get; set; }
        public string PaxTitle { get; set; }
        public string PaxFName { get; set; }
        public string PaxLName { get; set; }
        public int? NoOfRooms { get; set; }
        public int? RoomCode { get; set; }
        public int? RoomCatCode { get; set; }
        public string RoomCategory { get; set; }
        public string RoomNum { get; set; }
        public string PaxEmail { get; set; }
        public string PaxPhoneNumber { get; set; }
        public string PaxDescription { get; set; }
        public int? Adults { get; set; }
        public int? Children { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int? RoomRate { get; set; }
        public int? HotelCode { get; set; }
        public string ConfirmationNo { get; set; }
        public int? NoofDay { get; set; }
        public int? TotalRoomRate { get; set; }
    }
}




//[HttpPost]
//public ActionResult Thankyou(FormCollection frm)
//{
//    try
//    {
//        string confirmationId = Request.Form["pp_BillReference"];
//        string pp_ResponseCode = Request.Form["pp_ResponseCode"];
//        string pp_ResponseMessage = Request.Form["pp_ResponseMessage"];

//        if (pp_ResponseCode == "000")
//        {
//            var _booking = (from obj in HDE.Bookings
//                            join b in HDE.RoomCategories
//                             on obj.RoomCatCode equals b.Id
//                            where (obj.ConfirmationNo == confirmationId)
//                            select new FinalBooking
//                            {
//                                BookingID = obj.BookingID,
//                                PaxTitle = obj.PaxTitle,
//                                PaxFName = obj.PaxFName,
//                                PaxLName = obj.PaxLName,
//                                NoOfRooms = obj.NoOfRooms,
//                                RoomCode = obj.RoomCode,
//                                RoomCatCode = obj.RoomCatCode,
//                                RoomCategory = b.Category,
//                                RoomNum = obj.RoomNum,
//                                PaxEmail = obj.PaxEmail,
//                                PaxPhoneNumber = obj.PaxPhoneNumber,
//                                PaxDescription = obj.PaxDescription,
//                                Adults = obj.Adults,
//                                Children = obj.Children,
//                                CheckIn = obj.CheckIn,
//                                CheckOut = obj.CheckOut,
//                                RoomRate = obj.RoomRate,
//                                HotelCode = obj.HotelCode,
//                                ConfirmationNo = obj.ConfirmationNo,
//                                NoofDay = obj.NoofDay,
//                                TotalRoomRate = obj.TotalRoomRate
//                            }).ToList();

//            //RoomCates = RoomCates.Remove(0, 1);
//            //TempData["RoomCates"] = RoomCates;
//            //TempData["noOfDay"] = noOfDay;
//            Session["booking"] = _booking;
//            Session["confirmationId"] = confirmationId;

//            using (var db = new HotelDBEntities())
//            {
//                var myData = db.Bookings.Where(x => x.ConfirmationNo == confirmationId).ToList();
//                myData.ForEach(m => m.BookingStatus = "Booked");
//                db.SaveChanges();
//            }


//            TimeSpan t = Convert.ToDateTime(_booking[0].CheckOut) - Convert.ToDateTime(_booking[0].CheckIn);
//            int noOfDay = (int)t.TotalDays;

//            try
//            {
//                StringBuilder htmlString = new StringBuilder();
//                htmlString.Append(@"<html>");
//                htmlString.Append("<body>");
//                htmlString.Append("<h3>Thanks</h3>");
//                htmlString.Append("<p> New Order Received.</p>");
//                htmlString.Append("Confirmation Num.:  <b>" + confirmationId + "</b><br />");
//                htmlString.Append("<table border='1'><tr>");
//                htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
//                htmlString.Append("<tr><td>" + _booking[0].PaxTitle + " " + _booking[0].PaxFName + " " + _booking[0].PaxLName + " </td><td> " + _booking[0].PaxPhoneNumber + "</td>");
//                htmlString.Append("<td>" + _booking[0].CheckIn + "</td> ");
//                htmlString.Append("<td>" + _booking[0].CheckOut + "</td>");
//                htmlString.Append("</tr>");
//                htmlString.Append("</table><br />");
//                htmlString.Append("<span><b>Adults:</b></span><span>" + _booking[0].Adults + "</span><br />");
//                htmlString.Append("<span><b>Children:</b></span><span>" + _booking[0].Children + "</span><br />");
//                htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + _booking[0].NoOfRooms + "</span><br />");
//                htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
//                //htmlString.Append("<span><b>Total Amount:</b></span><span>" + _booking[0].TotalAmount + "</span><br />");
//                htmlString.Append("</body>");
//                htmlString.Append("</html>");

//                MailMessage mm = new MailMessage();
//                mm.IsBodyHtml = true;
//                mm.To.Add("info@mountfeasthotel.com");

//                mm.From = new MailAddress("growbiztec@gmail.com");
//                mm.Subject = "Order Confirmation: " + confirmationId;
//                mm.Body = htmlString.ToString();
//                SmtpClient smc = new SmtpClient();
//                smc.Host = "smtp.gmail.com";
//                smc.Port = 587;
//                // smc.UseDefaultCredentials = false;
//                smc.EnableSsl = true;
//                smc.Credentials = new System.Net.NetworkCredential("growbiztec@gmail.com", "growbiz@2022");
//                smc.DeliveryMethod = SmtpDeliveryMethod.Network;

//                smc.Send(mm);


//            }
//            catch (Exception ex)
//            {
//                TempData["error"] = ex.Message;
//            }

//            if (_booking[0].PaxEmail != "")
//            {
//                try
//                {
//                    StringBuilder htmlString = new StringBuilder();
//                    htmlString.Append(@"<html>");
//                    htmlString.Append("<body>");
//                    htmlString.Append("<h3>Thanks</h3>");
//                    htmlString.Append("<p> We have received your booking order and are working on a confirmation.</p>");
//                    htmlString.Append("Your Confirmation Num.:  <b>" + confirmationId + "</b><br />");
//                    htmlString.Append("<table border='1'><tr>");
//                    htmlString.Append("<th>Guest Nmae</th><th>Phone Num.</th><th>Room Category</th><th>CheckIn Date/Time</th><th>CheckOut</th></tr>");
//                    htmlString.Append("<tr><td>" + _booking[0].PaxTitle + " " + _booking[0].PaxFName + " " + _booking[0].PaxLName + " </td><td>" + _booking[0].PaxPhoneNumber + " </td>");
//                    // htmlString.Append("<td>" + RoomCates + "</td>");
//                    htmlString.Append("<td>" + _booking[0].CheckIn + "</td> ");
//                    htmlString.Append("<td>" + _booking[0].CheckOut + "</td>");
//                    htmlString.Append("</tr>");
//                    htmlString.Append("</table><br />");
//                    htmlString.Append("<span><b>Adults:</b></span><span>" + _booking[0].Adults + "</span><br />");
//                    htmlString.Append("<span><b>Children:</b></span><span>" + _booking[0].Children + "</span><br />");
//                    htmlString.Append("<span><b>No. Of Rooms:</b></span><span>" + _booking[0].NoOfRooms + "</span><br />");
//                    htmlString.Append("<span><b>No. Of Night:</b></span><span>" + noOfDay + "</span><br />");
//                    //htmlString.Append("<span><b>Total Amount:</b></span><span>" + TotalAmount + "</span><br />");
//                    htmlString.Append("</body>");
//                    htmlString.Append("</html>");

//                    MailMessage mm = new MailMessage();
//                    mm.IsBodyHtml = true;
//                    mm.To.Add(_booking[0].PaxEmail);

//                    mm.From = new MailAddress("info@mountfeasthotel.com");
//                    mm.Subject = "Order Confirmation: " + confirmationId;
//                    mm.Body = htmlString.ToString();
//                    SmtpClient smc = new SmtpClient();
//                    smc.Host = "mail.mountfeasthotel.com";
//                    smc.Port = 587;
//                    // smc.UseDefaultCredentials = false;
//                    smc.EnableSsl = true;
//                    smc.Credentials = new System.Net.NetworkCredential("info@mountfeasthotel.com", "Emirates321");
//                    smc.DeliveryMethod = SmtpDeliveryMethod.Network;
//                    smc.Send(mm);


//                }
//                catch (Exception ex)
//                {
//                    TempData["error"] = ex.Message;
//                }

//            }

//            return View();
//        }
//        else
//        {
//            return RedirectToAction("PaymentError", "SearchResult", new { @pp_ResponseCode = pp_ResponseCode, @pp_ResponseMessage = pp_ResponseMessage });
//        }



//    }
//    catch (Exception ex)
//    {

//        return Json(new { status = "error", message = ex.ToString() });
//    }
//}

//[HttpPost]
//public ActionResult SaveBooking()
//{
//    try
//    {
//        string PaxTitle = Request.Form["PaxTitle"];
//        string PaxFName = Request.Form["PaxFName"];
//        string PaxLName = Request.Form["PaxLName"];
//        string checkIn = Convert.ToString(Request.Form["CheckIn"]);
//        string checkOut = Convert.ToString(Request.Form["CheckOut"]);
//        int RoomCatCode = 0;  //Convert.ToInt32(Request.Form["RoomCatCode"]);
//        string PaxPhoneNumber = Request.Form["PaxPhoneNumber"];
//        string PaxEmail = Request.Form["PaxEmail"];
//        string PaxDescription = Request.Form["PaxDescription"];
//        int Adults = Convert.ToInt32(Request.Form["Adults"]);
//        int Children = Convert.ToInt32(Request.Form["Children"]);
//        int NoOfRooms = Convert.ToInt32(Request.Form["hdTotalRooms"]);
//        int RoomRate = 0; // Convert.ToInt32(Request.Form["RoomRate"]);
//        int TotalAmount = Convert.ToInt32(Request.Form["hdTotalAmount"]);
//        string RoomCatCodes = Request.Form["RoomCatCode"];
//        string RoomCateRoomNums = Request.Form["RoomCateRoomNums"];
//        string RatePlanIDs = Request.Form["RatePlanIDs"];
//        string RoomRates = Request.Form["RoomRates"];

//        string[] _CheckIn = checkIn.Split('-');
//        string[] _CheckOut = checkOut.Split('-');

//        checkIn = _CheckIn[1] + "/" + _CheckIn[0] + "/" + _CheckIn[2];
//        checkOut = _CheckOut[1] + "/" + _CheckOut[0] + "/" + _CheckOut[2];

//        DateTime CheckIn = Convert.ToDateTime(checkIn);
//        DateTime CheckOut = Convert.ToDateTime(checkOut);

//        if (Adults == 0)
//        {
//            Adults = 1;
//        }

//        using (var db = new HotelDBEntities())
//        {
//            var rowsAffected = db.Bookings.Where(x => x.PaxFName == PaxFName && x.PaxLName == PaxLName &&
//                        x.CheckIn == CheckIn).FirstOrDefault();

//            if (true == true)
//            {
//                int? bookingID = db.Bookings.DefaultIfEmpty().Max(p => p == null ? 0 : p.BookingID);
//                string confirmationId = "HMF-" + Convert.ToString(bookingID + 1);
//                int? PropertyId = db.Rooms.Where(p => p.RoomCategory == RoomCatCode).Select(p => p.propertyID).FirstOrDefault();
//                int? RoomCode = 0;
//                string RoomCate = "";
//                string RoomCates = "";
//                TimeSpan t = Convert.ToDateTime(CheckOut) - Convert.ToDateTime(CheckIn);
//                int noOfDay = (int)t.TotalDays;
//                string[] _RoomCatCodes;
//                string[] _RoomCateRoomNums;
//                string[] _RatePlanIDs;
//                string[] _RoomRates;
//                double GrandTotal = 0;

//                RoomRates = RoomRates.Remove(0, 1);
//                _RoomRates = RoomRates.Split(',');

//                RoomCatCodes = RoomCatCodes.Remove(0, 1);
//                _RoomCatCodes = RoomCatCodes.Split(',');

//                RatePlanIDs = RatePlanIDs.Remove(0, 1);
//                _RatePlanIDs = RatePlanIDs.Split(',');

//                RoomCateRoomNums = RoomCateRoomNums.Remove(0, 1);
//                _RoomCateRoomNums = RoomCateRoomNums.Split(',');

//                //foreach (var itm in _RoomCatCodes)
//                for (int itm = 0; itm < _RoomCatCodes.Length; itm++)
//                {
//                    int CateCode = Convert.ToInt32(_RoomCatCodes[itm]);
//                    RoomCode = db.Rooms.Where(p => p.RoomCategory == CateCode).Select(p => p.RoomCode).FirstOrDefault();
//                    RoomCate = db.RoomCategories.Where(p => p.Id == CateCode).Select(p => p.Category).FirstOrDefault();
//                    RoomCates = RoomCates + "," + RoomCate;

//                    //foreach (var itm1 in _RoomCateRoomNums)
//                    {
//                        int NoofRooms = Convert.ToInt32(_RoomCateRoomNums[itm]);

//                        for (int i = 1; i <= NoofRooms; i++)
//                        {
//                            var bookedRooms = db.Bookings.Where(x => x.RoomCode == RoomCode && x.CheckIn == CheckIn).Select(p => p.RoomNum).ToList();
//                            var RoomNum = (db.RoomNumbers.Where(p => p.RoomCode == RoomCode).Select(p => p.RoomNumber1).ToList());
//                            //var roomnum = db.RoomNumbers.Where(p => p.RoomNumber1 == RoomNum).FirstOrDefault();

//                            var roomnum = RoomNum.Except(bookedRooms).FirstOrDefault();

//                            if (roomnum != null)
//                            {
//                                //roomnum.Booked = true;

//                                //db.Entry(roomnum).State = System.Data.EntityState.Modified;
//                                //db.SaveChanges();

//                                GrandTotal = GrandTotal + TotalAmount;

//                                Booking booking = new Booking();

//                                booking.PaxTitle = PaxTitle;
//                                booking.PaxFName = PaxFName;
//                                booking.PaxLName = PaxLName;
//                                booking.NoOfRooms = 1;
//                                booking.RoomCode = RoomCode;
//                                booking.RoomCatCode = CateCode;
//                                booking.RoomNum = roomnum;
//                                booking.PaxEmail = PaxEmail;
//                                booking.PaxPhoneNumber = PaxPhoneNumber;
//                                booking.PaxDescription = PaxDescription;
//                                booking.Adults = Adults;
//                                booking.Children = Children;
//                                booking.CheckIn = CheckIn;
//                                booking.CheckOut = CheckOut;
//                                booking.RoomRate = Convert.ToInt32(_RoomRates[itm]);
//                                booking.TotalRoomRate = Convert.ToInt32(_RoomRates[itm]);
//                                booking.HotelCode = 5;
//                                booking.ConfirmationNo = confirmationId;
//                                booking.DateModified = DateTime.Now;
//                                booking.BookingStatus = "Pending";
//                                booking.NoofDay = noOfDay;
//                                booking.TotalRoomRate = TotalAmount;
//                                booking.ThemeColor = "red";
//                                booking.RatePlanId = Convert.ToInt32(_RatePlanIDs[itm]);

//                                db.Entry(booking).State = EntityState.Added;
//                                db.SaveChanges();
//                            }
//                        }
//                    }
//                }
//                return RedirectToAction("Payment", "Jazzcash", new { @billReference = confirmationId, @GrandTotal = GrandTotal });

//                // return ("Booking done succesfully");
//                //jsonString = JsonSerializer.Serialize(confirmationId);

//            }
//            else
//            {
//                // responseBasicProfile.MessageList.Add("Not Record Found.");
//                return RedirectToAction("Index", "Home");
//            }
//        }

//    }
//    catch (Exception ex)
//    {
//        //TempData["error"] = ex.Message;
//        return RedirectToAction("Index", "Home");
//    }
//}