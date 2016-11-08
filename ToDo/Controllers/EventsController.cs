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

namespace ToDo.Controllers
{
    public class EventsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Events
        public ActionResult Index()
        {
            var events = db.Events.Include(e => e.Venue);
            return View(events.ToList());
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
            ViewBag.ETown = venue.VenueTown;
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

            //Combine location into one string
            string address = string.Format("{0}, {1}, {2}", venue.VenueName, venue.VenueAddress, venue.VenueTown);

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

            //YouTube
            if (@event.EventYouTube != null)
            {
                ViewBag.hasYT = true;
                ViewBag.youTubeID  = @event.EventYouTube.Substring(@event.EventYouTube.LastIndexOf('=') + 1);
            }

            else
            {
                ViewBag.hasYT = false;
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

            return View(newEvent);
        }

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventID,VenueID,EventTitle,EventDate,EventTime,EventDescription,EventCategory,EventYouTube,EventSoundCloud,EventFacebook,EventTwitter,EventInstagram,EventWebsite,EventTicketPrice,EventTicketStore")] Event @event, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                //Get Owner Id
                string UserId = User.Identity.GetUserId();
                @event.OwnerID = UserId;

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
                return RedirectToAction("Index");
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
            if (@event == null)
            {
                return HttpNotFound();
            }
            ViewBag.VenueID = new SelectList(db.Venues, "VenueID", "OwnerId", @event.VenueID);
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventID,OwnerID,VenueID,EventTitle,EventDate,EventTime,EventDescription,EventCategory,EventYouTube,EventSoundCloud,EventFacebook,EventTwitter,EventInstagram,EventWebsite,EventTicketPrice,EventTicketStore")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.VenueID = new SelectList(db.Venues, "VenueID", "OwnerId", @event.VenueID);
            return View(@event);
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
            db.Events.Remove(@event);
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
    }
}
