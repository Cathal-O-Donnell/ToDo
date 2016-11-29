using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToDo.Models;

namespace ToDo.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        //Database
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            //See FeaturedEventPartialView/ FeaturedVenuesPartialView

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //Featured Event Partial View
        public ActionResult FeaturedEventPartialView()
        {
            //Hardcoded until Admin section is done where the event can be selected
            AdminSettings admin = db.AdminSettings.Find(1);

            List<Event> featuredEvents = new List<Event>();

           // Event featuredEvent1 = db.Events.Find(admin.FeaturedEvent1);
            Event featuredEvent1 = db.Events.Include(s => s.Files).SingleOrDefault(s => s.EventID == admin.FeaturedEvent1);
            Event featuredEvent2 = db.Events.Include(s => s.Files).SingleOrDefault(s => s.EventID == admin.FeaturedEvent2);
            Event featuredEvent3 = db.Events.Include(s => s.Files).SingleOrDefault(s => s.EventID == admin.FeaturedEvent3);

            featuredEvents.Add(featuredEvent1);
            featuredEvents.Add(featuredEvent2);
            featuredEvents.Add(featuredEvent3);

            return PartialView("_FeaturedEvent", featuredEvents);
        }

        //Featured Venues Partial View
        public ActionResult FeaturedVenuesPartialView()
        {
            AdminSettings admin = db.AdminSettings.Find(1);

            //Get Venues from database
            Venue featuredVenue1 = db.Venues.Find(admin.FeaturedVenue1);
            Venue featuredVenue2 = db.Venues.Find(admin.FeaturedVenue2);
            Venue featuredVenue3 = db.Venues.Find(admin.FeaturedVenue3);

            List<Venue> featuredVenues = new List<Venue>();

            featuredVenues.Add(featuredVenue1);
            featuredVenues.Add(featuredVenue2);
            featuredVenues.Add(featuredVenue3);

            return PartialView("_FeaturedVenue", featuredVenues.ToList());
        }

        //Admin
        public ActionResult Admin()
        {
            bool UserRole = User.IsInRole("Admin");

            if (UserRole == true)
            {
                var admin = db.AdminSettings.Find(1);

                //Top Event
                Event TopFeaturedEvent = db.Events.Find(admin.TopFeaturedEvent);
                ViewBag.TopEvent = TopFeaturedEvent.EventTitle;

                //Featured Events
                Event FeaturedEvent1 = db.Events.Find(admin.FeaturedEvent1);
                ViewBag.Event1 = FeaturedEvent1.EventTitle;

                Event FeaturedEvent2 = db.Events.Find(admin.FeaturedEvent2);
                ViewBag.Event2 = FeaturedEvent2.EventTitle;

                Event FeaturedEvent3 = db.Events.Find(admin.FeaturedEvent3);
                ViewBag.Event3 = FeaturedEvent3.EventTitle;

                //Top Venue
                Venue TopFeaturedVenue = db.Venues.Find(admin.TopFeaturedVenue);
                ViewBag.TopVenue = TopFeaturedVenue.VenueName;

                //Featured Venues
                Venue Venue1 = db.Venues.Find(admin.FeaturedVenue1);
                ViewBag.Venue1 = Venue1.VenueName;

                Venue Venue2 = db.Venues.Find(admin.FeaturedVenue2);
                ViewBag.Venue2 = Venue2.VenueName;

                Venue Venue3 = db.Venues.Find(admin.FeaturedVenue3);
                ViewBag.Venue3 = Venue3.VenueName;

                int EventCount = db.Events.Distinct().Count();
                ViewBag.EventCount = EventCount;

                int VenueCount = db.Venues.Distinct().Count();
                ViewBag.VenueCount = VenueCount;

                return View(admin);
            }

            else
                return RedirectToAction("Index", "Home");

        }

        [HttpGet]
        public ActionResult EditAdminSettings()
        {
            bool UserRole = User.IsInRole("Admin");

            if (UserRole == true)
            {
                //Get UserID
                string UserId = User.Identity.GetUserId();

                //User not logged in, redirect to Login Page
                if (UserId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                AdminSettings admin = db.AdminSettings.Find(1);
                SelectListModel slm = new SelectListModel();

                slm.AdminSettingsID = admin.AdminSettingsID;

                var model = new SelectListModel
                {
                    //Top Event
                    TopEventId = admin.TopFeaturedEvent,
                    TopEventOption = db.Events.Select(x => new SelectListItem
                    {
                        Value = x.EventID.ToString(),
                        Text = x.EventTitle
                    }),
                    //Featured Event 1
                    Event1Id = admin.FeaturedEvent1,
                    Event1_Option = db.Events.Select(x => new SelectListItem
                    {
                        Value = x.EventID.ToString(),
                        Text = x.EventTitle
                    }),
                    //Featured Event 2
                    Event2Id = admin.FeaturedEvent2,
                    Event2_Option = db.Events.Select(x => new SelectListItem
                    {
                        Value = x.EventID.ToString(),
                        Text = x.EventTitle
                    }),
                    //Featured Event 3
                    Event3Id = admin.FeaturedEvent3,
                    Event3_Option = db.Events.Select(x => new SelectListItem
                    {
                        Value = x.EventID.ToString(),
                        Text = x.EventTitle
                    }),
                    //Top Venue
                    TopVenueId = admin.TopFeaturedVenue,
                    TopVenueOption = db.Venues.Select(x => new SelectListItem
                    {
                        Value = x.VenueID.ToString(),
                        Text = x.VenueName
                    }),
                    //Featured Venue 1
                    Venue1Id = admin.FeaturedVenue1,
                    Venue1_Option = db.Venues.Select(x => new SelectListItem
                    {
                        Value = x.VenueID.ToString(),
                        Text = x.VenueName
                    }),
                    //Featured Venue 2
                    Venue2Id = admin.FeaturedVenue2,
                    Venue2_Option = db.Venues.Select(x => new SelectListItem
                    {
                        Value = x.VenueID.ToString(),
                        Text = x.VenueName
                    }),
                    //Featured Venue 3
                    Venue3Id = admin.FeaturedVenue3,
                    Venue3_Option = db.Venues.Select(x => new SelectListItem
                    {
                        Value = x.VenueID.ToString(),
                        Text = x.VenueName
                    }),
                };

                return View(model);
            }

            else
                return RedirectToAction("Index", "Home");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult EditAdminSettingsPost(int TopFeaturedEvent, int FeaturedEvent1, int FeaturedEvent2, int FeaturedEvent3, int TopFeaturedVenue, int FeaturedVenue1, int FeaturedVenue2, int FeaturedVenue3)
        {
            var admin = db.AdminSettings.Find(1);

            admin.TopFeaturedEvent = TopFeaturedEvent;
            admin.FeaturedEvent1 = FeaturedEvent1;
            admin.FeaturedEvent2 = FeaturedEvent2;
            admin.FeaturedEvent3 = FeaturedEvent3;

            admin.TopFeaturedVenue = TopFeaturedVenue;
            admin.FeaturedVenue1 = FeaturedVenue1;
            admin.FeaturedVenue2 = FeaturedVenue2;
            admin.FeaturedVenue3 = FeaturedVenue3;

            db.Entry(admin).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
            //return View("Admin");
        }
    }
}