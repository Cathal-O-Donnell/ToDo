using System;
using System.Collections.Generic;
using System.Linq;
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
            //See FeaturedEventPartialView

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
            int id = 42;

            Event featuredEvent = db.Events.Find(id);

            return PartialView("_FeaturedEvent", featuredEvent);
        }

        //Featured Venues Partial View
        public ActionResult FeaturedVenuesPartialView()
        {
            //Hardcoded until Admin section is done where the event can be selected
            int Venue1 = 15;
            int Venue2 = 16;
            int Venue3 = 17;

            //Get Venues from database
            Venue featuredVenue1 = db.Venues.Find(Venue1);
            Venue featuredVenue2 = db.Venues.Find(Venue2);
            Venue featuredVenue3 = db.Venues.Find(Venue3);

            List<Venue> featuredVenues = new List<Venue>();

            featuredVenues.Add(featuredVenue1);
            featuredVenues.Add(featuredVenue2);
            featuredVenues.Add(featuredVenue3);

            return PartialView("_FeaturedVenue", featuredVenues.ToList());
        }
    }
}