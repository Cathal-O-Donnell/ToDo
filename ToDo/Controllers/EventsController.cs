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

namespace ToDo.Controllers
{
    [RequireHttps]
    public class EventsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Events
        public ActionResult Index()
        {
            //See EventsTablePartialView
            return View();
        }

        // GET: Events/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Get selected event from DB
            Event @event = db.Events.Find(id);

            //Get Venue Id
            int venueID = @event.VenueID;
            //Find venue in db
            Venue venue = db.Venues.Find(venueID);
            //Pass venue info to view
            ViewBag.EVenue = venue.VenueName;
            ViewBag.EAddress = venue.VenueAddress;
            ViewBag.ETown = db.Towns.Find(venue.VenueTownID).TownName;
            //ViewBag.ETown = venue.VenueTown;
            ViewBag.EPhone = venue.VenuePhoneNumber;
            ViewBag.EEmail = venue.VenueEmail;

            //Get currents users id
            string UserId = User.Identity.GetUserId();

            //Check if current user is the owner of this event
            if (@event.OwnerID == UserId)
            {
                ViewBag.IsOwner = true;
            }

            //Check if the event is free
            if (@event.EventTicketPrice == null || @event.EventTicketPrice == 0)
            {
                ViewBag.FreeEvent = true;
            }

            else
            {
                ViewBag.FreeEvent = false;
            }

            //GeoCodder: Long and Lat values
            IGeocoder geoCode;

            geoCode = new GoogleGeocoder();

            string twn = db.Towns.Find(venue.VenueTownID).TownName;

            //Combine location into one string
            string address = string.Format("{0}, {1}, {2}", venue.VenueName, venue.VenueAddress, twn);

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

            //Get Event Image
            @event = db.Events.Include(s => s.Files).SingleOrDefault(s => s.EventID == id);

            //FaceBook
            if (@event.EventFacebook != null)
            {
                ViewBag.hasFB = true;
                ViewBag.FaceBook = @event.EventFacebook;
            }

            else
            {
                ViewBag.hasFB = false;
            }
            

            //YouTube
            if (@event.EventYouTube != null)
            {
                ViewBag.hasYT = true;
                ViewBag.youTubeID = @event.EventYouTube.Substring(@event.EventYouTube.LastIndexOf('=') + 1);
            }

            else
            {
                ViewBag.hasYT = false;
            }

            //Soundcloud
            if (@event.EventSoundCloud != null)
            {
                ViewBag.hasSC = true;
                ViewBag.SoundCloud = @event.EventSoundCloud;
            }

            else
            {
                ViewBag.hasSC = false;
            }

            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // GET: Events/Create
        public ActionResult Create(int? id)
        {

            //Get the currentely logged in user
            string currentUserId = User.Identity.GetUserId();

            //If no user is logged in, redirect them to the login page
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.VID = id;

            //Get the currentely selected club
            var venue = db.Venues.Find(id);

            if (venue == null)
            {
                return HttpNotFound();
            }

            Event newEvent = new Event() { Venue = venue, VenueID = Convert.ToInt32(id) };

            //Venue Types
            ViewBag.EventCategories = new SelectList(db.EventCategories.OrderBy(x => x.EventCategoryName), "EventCategoryID", "EventCategoryName");

            return View(newEvent);
        }

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventID,VenueID,EventTitle,EventDate,EventTime,EventEndTime,EventDescription,EventYouTube,EventSoundCloud,EventFacebook,EventTwitter,EventInstagram,EventWebsite,EventTicketPrice,EventTicketStore,EventCatID")] Event @event, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                //Get Owner Id
                string UserId = User.Identity.GetUserId();
                @event.OwnerID = UserId;

                //Get the new id for this event
                var NextId = this.db.Events.Max(t => t.EventID);
                var newId = NextId + 1;
                @event.EventID = newId;

                //Set this Events status as active
                @event.EventActive = true;

                //Event Category
                @event.EventCat = db.EventCategories.Find(@event.EventCatID);

                //Image File Upload
                if (imageUpload != null && imageUpload.ContentLength > 0)
                {
                    var imgEvent = new File
                    {
                        FileName = System.IO.Path.GetFileName(imageUpload.FileName),
                        FileType = FileType.EventImage,
                        ContentType = imageUpload.ContentType
                    };
                    using (var reader = new System.IO.BinaryReader(imageUpload.InputStream))
                    {
                        imgEvent.Content = reader.ReadBytes(imageUpload.ContentLength);
                    }

                    @event.Files = new List<File> { imgEvent };
                }

                db.Events.Add(@event);
                db.SaveChanges();
                
                //Redirect to details view for the new event
                return RedirectToAction("Details", new
                {
                    id = @event.EventID
                });
        }

            ViewBag.VenueID = new SelectList(db.Venues, "VenueID", "OwnerId", @event.VenueID);
            return View(@event);
        }

        // GET: Events/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Event @event = db.Events.Find(id);

            //Image
            @event = db.Events.Include(s => s.Files).SingleOrDefault(s => s.EventID == id);

            //Owner ID
            ViewBag.OID = @event.OwnerID;

            //Event Catagories
            ViewBag.EventCatagories = new SelectList(db.EventCategories.OrderBy(x => x.EventCategoryName), "EventCategoryID", "EventCategoryName");

            int eventCatID = @event.EventCatID;

            if (@event == null)
            {
                return HttpNotFound();
            }
            ViewBag.VenueID = new SelectList(db.Venues, "VenueID", "OwnerId", @event.VenueID);


            return View(@event);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, HttpPostedFileBase upload)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var EventToUpdate = db.Events.Find(id);
            if (TryUpdateModel(EventToUpdate, "",
                new string[] { "EventID","OwnerID","VenueID","EventTitle","EventDate","EventTime", "EventEndTime", "EventDescription", "EventCatID", "EventYouTube","EventSoundCloud","EventFacebook","EventTwitter","EventInstagram","EventWebsite","EventTicketPrice","EventTicketStore" }))
            {
                try
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        if (EventToUpdate.Files.Any(f => f.FileType == FileType.EventImage))
                        {
                            db.Files.Remove(EventToUpdate.Files.First(f => f.FileType == FileType.EventImage));
                        }
                        var img = new File
                        {
                            FileName = System.IO.Path.GetFileName(upload.FileName),
                            FileType = FileType.EventImage,
                            ContentType = upload.ContentType
                        };
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            img.Content = reader.ReadBytes(upload.ContentLength);
                        }
                        EventToUpdate.Files = new List<File> { img };
                    }

                    Event oldEvent = db.Events.Find(EventToUpdate.EventID);

                    EventToUpdate.Venue = oldEvent.Venue;
                    EventToUpdate.VenueID = oldEvent.VenueID;
                    EventToUpdate.Files = oldEvent.Files;

                    db.Entry(EventToUpdate).State = EntityState.Modified;
                    db.SaveChanges();

                    //Event Category
                    Event eventA = db.Events.Find(EventToUpdate.EventID);
                    eventA.EventCat = db.EventCategories.Find(EventToUpdate.EventCatID);

                    db.SaveChanges();

                    

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(EventToUpdate);
        }

        // GET: Events/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Event @event = db.Events.Find(id);
            int venueID = @event.VenueID;
            //db.Events.Remove(@event);

            //Set this event as inactive
            @event.EventActive = false;

            db.SaveChanges();

            //Redirect to details view for the current venue
            return RedirectToAction("Details", "Venues", new
            {
                id = venueID
            });
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
        public ActionResult EventsTablePartialView(string search, string Town, string EventCategory, string AdvancedSearch, string TownIndex, string CategoryIndex)
        {
            ViewBag.TownIndex = TownIndex;
            ViewBag.CategoryIndex = CategoryIndex;

            ViewBag.Towns = new SelectList(db.Towns.OrderBy(x => x.TownName), "TownId", "TownName");
            ViewBag.EventCategories = new SelectList(db.EventCategories.OrderBy(x => x.EventCategoryName), "EventCategoryID", "EventCategoryName");

            //Get all events from the database
            var events = from e in db.Events
                         where e.EventActive == true && e.Venue.VenueActive == true
                         select e;

            if (AdvancedSearch == "true")
            {

                events = from v in db.Events
                         where v.EventActive == true
                         select v;

                if (Town != "")
                {
                    int townID = Convert.ToInt32(Town);
                    events = events.Where(e => e.Venue.VenueTown.TownID == townID);
                }

                if (EventCategory != "")
                {
                    int EventCategoryID = Convert.ToInt32(EventCategory);
                    events = events.Where(e => e.EventCat.EventCategoryID == EventCategoryID);
                }

                if (!String.IsNullOrEmpty(search))
                {
                    //Get all the events where the name contains the users search term
                    events = events.Where(e => e.EventTitle.ToUpper().Contains(search.ToUpper()));
                    ViewBag.SearchTerm = search;
                }

                return PartialView("_EventsTable", events.OrderBy(e => e.EventTitle).ToList());
            }


            else
            {
                //Search Bar
                if (!String.IsNullOrEmpty(search))
                {
                    //Get all the events where the name contains the users search term
                    events = events.Where(e => e.EventTitle.ToUpper().Contains(search.ToUpper()));
                    ViewBag.SearchTerm = search;
                }

                return PartialView("_EventsTable", events.OrderBy(v => v.EventTitle).ToList());
            }
        }
    }
}
