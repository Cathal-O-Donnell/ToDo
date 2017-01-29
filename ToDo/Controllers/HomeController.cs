using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
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
            //See FeaturedEventPartialView/ FeaturedVenuesPartialView/ TopEvent

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

        //Top Event Partial View
        public ActionResult TopEventPartialView()
        {
            AdminSettings admin = db.AdminSettings.Find(1);

            //Get Top Event from database
            Event topEvent = db.Events.Find(admin.TopFeaturedEvent);

            return PartialView("_TopEvent", topEvent);
        }

        //Top Venue Partial View
        public ActionResult TopVenuePartialView()
        {
            AdminSettings admin = db.AdminSettings.Find(1);

            //Get Top Event from database
            Venue topVenue = db.Venues.Find(admin.TopFeaturedVenue);

            return PartialView("_TopVenue", topVenue);
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

                //Event Count
                int EventCount = db.Events.Distinct().Count();
                ViewBag.EventCount = EventCount;

                //Venue Count
                int VenueCount = db.Venues.Distinct().Count();
                ViewBag.VenueCount = VenueCount;

                //Registerd User Count
                var userCount = db.Users.Count();
                ViewBag.UserCount = userCount;

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
        }

        //Admin Venue Partial View
        public ActionResult AdminVenuesTablePartialView()
        {
            var venues = from v in db.Venues
                         select v;

            return PartialView("_AdminVenues", venues.OrderBy(v => v.VenueName).ToList());
        }

        //Admin EVent Partial View
        public ActionResult AdminEventsTablePartialView()
        {

            var events = from e in db.Events
                         select e;

            return PartialView("_AdminEvents", events.OrderBy(e => e.EventTitle).ToList());
        }

        // Remove Event 
        public ActionResult RemoveEvent(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Event e = db.Events.Find(id);

            if (e == null)
            {
                return HttpNotFound();
            }

            db.Events.Remove(e);
            db.SaveChanges();

            var ev = from events in db.Events
                         select events;

            return PartialView("_AdminEvents", ev.OrderBy(x => x.EventTitle).ToList());
            //return RedirectToAction("Admin");
            //return View(venue)
        }

        // Remove Venue 
        public ActionResult RemoveVenue(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Venue venue = db.Venues.Find(id);

            if (venue == null)
            {
                return HttpNotFound();
            }

            db.Venues.Remove(venue);
            db.SaveChanges();

            var venues = from v in db.Venues
                         select v;

            return PartialView("_AdminVenues", venues.OrderBy(v => v.VenueName).ToList());
            //return RedirectToAction("Admin");
            //return View(venue)
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("RemoveVenue")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Find(id);
            db.Venues.Remove(venue);
            db.SaveChanges();
            return RedirectToAction("Admin");
        }

        //Event Change Status
        public ActionResult EventChangeStatus(int id)
        {
            Event @event = db.Events.Find(id);

            //Change from active to inactive
            if (@event.EventActive == true)
            {
                @event.EventActive = false;
            }

            //Change from actice to inactive
            else
            {
                @event.EventActive = true;
            }

            db.Entry(@event).State = EntityState.Modified;
            db.SaveChanges();


            var events = from e in db.Events
                         select e;

            return PartialView("_AdminEvents", events.OrderBy(e => e.EventTitle).ToList());
        }

        //Venue Change Status
        public ActionResult VenueChangeStatus(int id)
        {
            Venue venue = db.Venues.Find(id);

            //Change from active to inactive
            if (venue.VenueActive == true)
            {
                venue.VenueActive = false;
            }

            //Change from actice to inactive
            else
            {
                venue.VenueActive = true;
            }

            db.Entry(venue).State = EntityState.Modified;
            db.SaveChanges();

            var venues = from v in db.Venues
                         select v;

            return PartialView("_AdminVenues", venues.OrderBy(v => v.VenueName).ToList());
            //return RedirectToAction("Admin");
        }

        //Admin Venue Partial View
        public PartialViewResult AdminUsersTablePartialView()
        {
            //Get the current user
            if (User.Identity.IsAuthenticated)
            {
                string name = User.Identity.Name;
            }

            try
            {
                var userRoles = new List<RolesViewModel>();
                var context = new ApplicationDbContext();
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);

                //Get all the usernames
                foreach (var user in userStore.Users)
                {
                    var r = new RolesViewModel
                    {
                        UserName = user.UserName,
                        Id = user.Id
                    };
                    userRoles.Add(r);
                }
                //Get all the Roles for our users
                foreach (var user in userRoles)
                {
                    user.RoleNames = userManager.GetRoles(userStore.Users.First(s => s.UserName == user.UserName).Id);
                }

                return PartialView("_AdminUsers", userRoles);
            }

            catch (Exception)
            {
                throw;
            }
        }

        public PartialViewResult AdminTownsTablePartialView()
        {
            var towns = from t in db.Towns
                        select t;

            return PartialView("_AdminTowns", towns.OrderBy(v => v.TownName).ToList());
        }

        [HttpGet]
        public ActionResult AddTown(string twn, string cty)
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

                Town t = new Town();

                t.TownName = twn;
                t.County = cty;

                db.Towns.Add(t);
                db.SaveChanges();

                var towns = from twns in db.Towns
                            select twns;

                return PartialView("_AdminTowns", towns.OrderBy(v => v.TownName).ToList());
                //return RedirectToAction("Admin", "Home");
            }

            else
                return RedirectToAction("Index", "Home");
        }

        //Remove town 
        public ActionResult RemoveTown(int id)
        {
            Town t = db.Towns.Find(id);
            db.Towns.Remove(t);
            db.SaveChanges();

            var towns = from twns in db.Towns
                        select twns;

            return PartialView("_AdminTowns", towns.OrderBy(v => v.TownName).ToList());

            //return RedirectToAction("Admin", "Home");
        }

        public PartialViewResult AdminVenueTypesTablePartialView()
        {
            var venueTypes = from vt in db.VenueCategories
                        select vt;

            return PartialView("_AdminVenueType", venueTypes.OrderBy(vt => vt.VenueTypeName).ToList());
        }

        [HttpGet]
        public ActionResult AddVenueType(string venueType)
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

                Venue_Type vt = new Venue_Type();

                vt.VenueTypeName = venueType;

                db.VenueCategories.Add(vt);
                db.SaveChanges();

                var venueTypes = from vtype in db.VenueCategories
                                 select vtype;

                return PartialView("_AdminVenueType", venueTypes.OrderBy(vtype => vtype.VenueTypeName).ToList());
                //return RedirectToAction("Admin", "Home");
            }

            else
            {
                var venueTypes = from vtype in db.VenueCategories
                                 select vtype;

                return PartialView("_AdminVenueType", venueTypes.OrderBy(vtype => vtype.VenueTypeName).ToList());
            }
        }

        //Remove Venue Type 
        public ActionResult RemoveVenueType(int id)
        {
            Venue_Type vt = db.VenueCategories.Find(id);
            db.VenueCategories.Remove(vt);
            db.SaveChanges();

            var venueTypes = from vtype in db.VenueCategories
                             select vtype;

            return PartialView("_AdminVenueType", venueTypes.OrderBy(vtype => vtype.VenueTypeName).ToList());
        }

        public PartialViewResult AdminEventCategoriesTablePartialView()
        {
            var eventCategories = from ecat in db.EventCategories
                             select ecat;

            return PartialView("_AdminEventCategories", eventCategories.OrderBy(ecat => ecat.EventCategoryName).ToList());
        }


        [HttpGet]
        public ActionResult AddVEventCategory(string Category)
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

                EventCategory ec = new EventCategory();

                ec.EventCategoryName = Category;

                db.EventCategories.Add(ec);
                db.SaveChanges();

                var eventCategories = from ecat in db.EventCategories
                                      select ecat;

                return PartialView("_AdminEventCategories", eventCategories.OrderBy(ecat => ecat.EventCategoryName).ToList());
                //return RedirectToAction("Admin", "Home");
            }

            else
            {
                var eventCategories = from ecat in db.EventCategories
                                      select ecat;

                return PartialView("_AdminEventCategories", eventCategories.OrderBy(ecat => ecat.EventCategoryName).ToList());
            }
        }

        //Remove Event Category 
        public ActionResult RemoveEventCategory(int id)
        {
            EventCategory ec = db.EventCategories.Find(id);
            db.EventCategories.Remove(ec);
            db.SaveChanges();

            var eventCategories = from ecat in db.EventCategories
                                  select ecat;

            return PartialView("_AdminEventCategories", eventCategories.OrderBy(ecat => ecat.EventCategoryName).ToList());
            //return RedirectToAction("Admin", "Home");
        }


        //Band
        public PartialViewResult AdminBandsPartialView()
        {
            var bands = from b in db.Bands
                                  select b;

            return PartialView("_AdminBands", bands.OrderBy(b => b.BandName).ToList());
        }

        //Remove Band
        public ActionResult RemoveBand(int id)
        {
            Band band = db.Bands.Find(id);

            db.Bands.Remove(band);
            db.SaveChanges();

            var bands = from b in db.Bands
                        select b;

            return PartialView("_AdminBands", bands.OrderBy(b => b.BandName).ToList());
        }

        //Band Change Status
        public ActionResult BandChangeStatus(int id)
        {
            Band band = db.Bands.Find(id);

            //Change from active to inactive
            if (band.BandActive == true)
            {
                band.BandActive = false;
            }

            //Change from actice to inactive
            else
            {
                band.BandActive = true;
            }

            db.Entry(band).State = EntityState.Modified;
            db.SaveChanges();


            var bands = from b in db.Bands
                        select b;

            return PartialView("_AdminBands", bands.OrderBy(b => b.BandName).ToList());
        }
    }
}