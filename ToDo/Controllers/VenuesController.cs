using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToDo.Models;

//Google Maps API
using Geocoding;
using Geocoding.Google;
using System.Data.Entity.Infrastructure;
using System.Web.Security;

namespace ToDo.Controllers
{
    [RequireHttps]
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venues
        public ActionResult Index(string AdvancedSearch, string Town, string VenueType)
        {

            ViewBag.LinkText = "Venues";

            //See VenuesTablePartialView

            if (AdvancedSearch != null)
            {
                TempData["AdvancedSearch"] = AdvancedSearch;
            }

            if (Town != null)
            {
                TempData["Town"] = Town;
            }

            if (VenueType != null)
            {
                TempData["VenueType"] = VenueType;
            }

            //Get UserID
            string UserId = User.Identity.GetUserId();

            //Check if the user is logged in
            if (UserId != null)
            {
                ViewBag.LoggedIn = true;
            }

            return View();
        }

        // GET: Venues/Details/5
        public ActionResult Details(int? id)
        {

            ViewBag.LinkText = "Venues";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Venue venue = db.Venues.Find(id);
            ViewBag.VenueID = venue.VenueID;            

            if (venue == null)
            {
                return HttpNotFound();
            }

            //Get UserID
            string UserId = User.Identity.GetUserId();

            //Check if current user is the owner of this event
            if (venue.OwnerId == UserId)
            {
                ViewBag.IsOwner = true;
            }

            else
            {
                ViewBag.IsOwner = false;

                //If user is not owner of this venue, add 1 to the view counter
                venue.VenueViewCounter = venue.VenueViewCounter + 1;

                //Reset the daily view counter
                if (DateTime.Now.Date != venue.VenueViewCounterReset)
                {
                    venue.VenueViewCounterReset = DateTime.Now.Date;
                    venue.VenueDailyViewCounter = 0;
                }

                //Incremente the daily view counter
                venue.VenueDailyViewCounter = venue.VenueDailyViewCounter + 1;

                db.SaveChanges();
            }

            //Get Events
            var events = (from e in db.Events
                          where e.VenueID == id && e.EventActive == true
                          select e).OrderBy(x => x.EventDate).ToList();

            venue.VenueEvents = events;

            //Sort Events
            foreach (var Event in venue.VenueEvents.ToList())
            {
                var EventDate = Event.EventDate;

                TimeSpan ts = DateTime.Now - EventDate;

                //Remove event if it is old
                if (ts.TotalDays > 1)
                {
                    db.Events.Remove(Event);
                    db.SaveChanges();
                }

                //Check if this event is schedluded for today
                if (EventDate == DateTime.Today)
                {
                    ViewBag.EventToday = 1;
                    ViewBag.TodaysEventTitle = Event.EventTitle;
                    ViewBag.EventStart = Event.EventTime.ToString("hh:mm tt");

                    //Check if Event has not started
                    if (DateTime.Now.TimeOfDay < Event.EventTime.TimeOfDay)
                    {
                        ViewBag.HasEventStarted = false;
                    }
                    //Check if Event has started
                    if (DateTime.Now.TimeOfDay > Event.EventTime.TimeOfDay)
                    {
                        ViewBag.HasEventStarted = true;
                    }
                }
            }

            //Check if there is any events for this Venue
            if (events.Count() < 1)
            {
                ViewBag.noEvents = true;
            }

            IGeocoder geoCode;
            geoCode = new GoogleGeocoder();

            //Combine location into one string
            string address = string.Format("{0}, {1}, {2}, Ireland", venue.VenueAddress, venue.VenueTown.TownName, venue.VenueTown.County);

            var coordinates = geoCode.Geocode(address).ToList();

            //Check if coordinates are valid
            if (coordinates.Count > 0)
            {
                var longlat = coordinates.First();

                //Pass variables to View
                ViewBag.Long = Convert.ToDouble(longlat.Coordinates.Longitude);
                ViewBag.Lat = Convert.ToDouble(longlat.Coordinates.Latitude);
                ViewBag.Address = address;

                ViewBag.hasMap = true;
            }

            return View(venue);
        }

        // GET: Venues/Create
        public ActionResult Create()
        {

            ViewBag.LinkText = "Venues";

            //Get UserID
            string UserId = User.Identity.GetUserId();

            //User not logged in, redirect to Login Page
            if (UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }


            else
            {
                //Towns
                ViewBag.TownId = new SelectList(db.Towns.OrderBy(x => x.TownName), "TownId", "TownName");

                //Venue Types
                ViewBag.VenueTypes = new SelectList(db.VenueCategories.OrderBy(x => x.VenueTypeName), "Venue_TypeID", "VenueTypeName");

                return View();
            }

        }

        // POST: Venues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VenueID,VenueName,VenueType,VenueAddress,VenueDescription,VenueEmail,VenuePhoneNumber,VenueTownID,VenueTypeID")] Venue venue, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                //Get the currentely logged in user
                string UserId = User.Identity.GetUserId();

                //User not logged in, redirect to Login Page
                if (UserId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                venue.VenueType = db.VenueCategories.Find(venue.VenueTypeID);
                venue.VenueTown = db.Towns.Find(venue.VenueTownID);

                venue.OwnerId = UserId;

                //Set the venue status as active
                venue.VenueActive = true;

                Town twn = db.Towns.Find(venue.VenueID);


                //Image File Upload
                if (imageUpload != null && imageUpload.ContentLength > 0)
                {
                    var imgVenue = new VenueFile
                    {
                        VenueFileName = System.IO.Path.GetFileName(imageUpload.FileName),
                        VenueFileType = FileType.EventImage,
                        VenueContentType = imageUpload.ContentType
                    };
                    using (var reader = new System.IO.BinaryReader(imageUpload.InputStream))
                    {
                        imgVenue.VenueContent = reader.ReadBytes(imageUpload.ContentLength);
                    }

                    venue.VenueFiles = new List<VenueFile> { imgVenue };
                }

                db.Venues.Add(venue);
                db.SaveChanges();

                //Redirect to details view for the new event
                return RedirectToAction("Details", new
                {
                    id = venue.VenueID
                });
            }

            return View(venue);
        }

        // GET: Venues/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.LinkText = "Venues";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }

            //Owner Id
            ViewBag.OID = venue.OwnerId;

            //Towns
            ViewBag.TownId = new SelectList(db.Towns, "TownID", "TownName");

            //Venue Types
            ViewBag.VenueTypes = new SelectList(db.VenueCategories.OrderBy(x => x.VenueTypeName), "Venue_TypeID", "VenueTypeName");

            int twnID = venue.VenueTownID;

            //Image
            venue = db.Venues.Include(s => s.VenueFiles).SingleOrDefault(s => s.VenueID == id);

            return View(venue);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, HttpPostedFileBase upload)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var venueToUpdate = db.Venues.Find(id);

            if (TryUpdateModel(venueToUpdate, "",
                new string[] { "VenueID", "OwnerId", "VenueName", "VenueTypeID", "VenueTownID", "VenueAddress", "VenueDescription", "VenueEmail", "VenuePhoneNumber" }))
            {
                try
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        if (venueToUpdate.VenueFiles.Any(f => f.VenueFileType == FileType.EventImage))
                        {
                            db.VenueFiles.Remove(venueToUpdate.VenueFiles.First(f => f.VenueFileType == FileType.EventImage));
                        }
                        var img = new VenueFile
                        {
                            VenueFileName = System.IO.Path.GetFileName(upload.FileName),
                            VenueFileType = FileType.EventImage,
                            VenueContentType = upload.ContentType
                        };
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            img.VenueContent = reader.ReadBytes(upload.ContentLength);
                        }
                        venueToUpdate.VenueFiles = new List<VenueFile> { img };
                    }

                    db.Entry(venueToUpdate).State = EntityState.Modified;
                    db.SaveChanges();

                    //Town and Venue Type 
                    Venue v = db.Venues.Find(venueToUpdate.VenueID);
                    v.VenueTown = db.Towns.Find(venueToUpdate.VenueTownID);
                    v.VenueType = db.VenueCategories.Find(venueToUpdate.VenueTypeID);

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(venueToUpdate);
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

            return RedirectToAction("Index");
        }

        // GET: Venues/Delete/5
        public ActionResult Delete(int? id)
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
            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Find(id);
            //db.Venues.Remove(venue);

            //Set this Venue as inactive
            venue.VenueActive = false;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Venue Index Partial View
        public ActionResult VenuesTablePartialView(string search, string Town, string VenueType, string AdvancedSearch, string TownIndex, string TypeIndex)
        {

            ViewBag.TownIndex = TownIndex;
            ViewBag.TypeIndex = TypeIndex;

            if (VenueType == null)
            {
                VenueType = "";
            }

            if (TempData["Town"] != null)
            {
                Town = Convert.ToString(TempData["Town"]);
            }

            if (TempData["VenueType"] != null)
            {
                VenueType = Convert.ToString(TempData["VenueType"]);
            }

            if (TempData["AdvancedSearch"] != null)
            {
                AdvancedSearch = Convert.ToString(TempData["AdvancedSearch"]);
            }


            ViewBag.Towns = new SelectList(db.Towns.OrderBy(x => x.TownName), "TownId", "TownName");
            ViewBag.VenueTypes = new SelectList(db.VenueCategories.OrderBy(x => x.VenueTypeName), "Venue_TypeID", "VenueTypeName");

            var venues = from v in db.Venues
                         where v.VenueActive == true
                         select v;

            if (AdvancedSearch == "true")
            {

                venues = from v in db.Venues
                         where v.VenueActive == true
                         select v;

                if (Town != "")
                {
                    int townID = Convert.ToInt32(Town);
                    venues = venues.Where(v => v.VenueTown.TownID == townID);
                }

                if (VenueType != "")
                {
                    int venueTypeID = Convert.ToInt32(VenueType);
                    venues = venues.Where(v => v.VenueType.Venue_TypeID == venueTypeID);
                }

                if (!String.IsNullOrEmpty(search))
                {
                    venues = venues.Where(v => v.VenueName.ToUpper().Contains(search.ToUpper()));
                    ViewBag.SearchTerm = search;
                }

                //Clear the TempData
                TempData["AdvancedSearch"] = null;
                TempData["Town"] = null;

                if (venues.Count() <= 0)
                {
                    ViewBag.NoVenues = true;
                }

                else
                {
                    ViewBag.NoVenues = false;
                }

                return PartialView("_VenuesTable", venues.OrderBy(v => v.VenueName).ToList());
            }

            else
            {
                //Search Bar
                if (!String.IsNullOrEmpty(search))
                {
                    venues = venues.Where(v => v.VenueName.ToUpper().Contains(search.ToUpper()));
                    ViewBag.SearchTerm = search;
                }

                if (venues.Count() <= 0)
                {
                    ViewBag.NoVenues = true;
                }

                else
                {
                    ViewBag.NoVenues = false;
                }

                return PartialView("_VenuesTable", venues.OrderBy(v => v.VenueName).ToList());
            }
        }



        //Venue Index Partial View
        public ActionResult VenuesEventsPartialView(int? VenueID, string search, string CategoryIndex, string EventCategory, DateTime? Date)
        {
            ViewBag.VenueID = VenueID;
            ViewBag.CategoryIndex = CategoryIndex;

            //Get Events
            var events = (from e in db.Events
                          where e.VenueID == VenueID && e.EventActive == true
                          select e).ToList();

            //Venue Types
            ViewBag.EventCategories = new SelectList(db.EventCategories.OrderBy(x => x.EventCategoryName), "EventCategoryID", "EventCategoryName");

            if (Date != null)
            {
                var stringDate = String.Format("{0:dd-MM-yyyy}", Date);
                DateTime formatedDate = DateTime.Parse(stringDate);
                events = events.Where(e => e.EventDate == formatedDate).ToList();
            }

            if (!String.IsNullOrEmpty(search))
            {
                //Get all the events where the name contains the users search term
                events = events.Where(e => e.EventTitle.ToUpper().Contains(search.ToUpper())).ToList();
                ViewBag.SearchTerm = search;
            }

            if (!String.IsNullOrEmpty(EventCategory))
            {
                int EventCategoryID = Convert.ToInt32(EventCategory);
                events = events.Where(e => e.EventCat.EventCategoryID == EventCategoryID).ToList();
            }

            return PartialView("_VenueEvents", events.OrderBy(v => v.EventTitle).ToList());
        }

        //Venue Mailing List
        public ActionResult VenueMailingList(int id)
        {
            //Get UserID and Email
            string UserId = User.Identity.GetUserId();
            string email = User.Identity.Name;

            //Check if the user is logged in
            if (UserId != null)
            {
                ViewBag.LoggedIn = true;

                //Add the users email to this venues email mailing list
                Venue venue = db.Venues.Find(id);

                //If list is null, initlise it
                //if (venue.VenueMailingList == null)
                //{
                //    venue.VenueMailingList = new List<string>();
                //}

                //venue.VenueMailingList.Add(email);

                db.SaveChanges();
            }


            return View();
        }
    }
}
