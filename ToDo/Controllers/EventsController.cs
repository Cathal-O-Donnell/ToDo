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
            //Count Events
            int numberOfEvents = db.Events.Count();
            ViewBag.numberOfEvents = numberOfEvents;

            return View(db.Events.ToList());
        }

        // GET: Events/Details/5
        public ActionResult Details(int? id)
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
            string address = string.Format("{0}, {1}, {2}", @event.EventVenue, @event.EventAddress, @event.EventTown);

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

            return View(@event);
        }

        // GET: Events/Create
        public ActionResult Create()
        {
            //Get UserID
            string UserId = User.Identity.GetUserId();

            //User not logged in, redirect to Login Page
            if (UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            else
            {
                return View();
            }
        }

        // POST: Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventID,OwnerID,EventTitle,EventTown,EventVenue,EventAddress,EventDate,EventTime,EventDescription,EventCategory,EventYouTube,EventSoundCloud,EventFacebook,EventTwitter,EventInstagram,EventWebsite,EventTicketPrice,EventTicketStore,EventImage,EventPhoneNumber,EventEmail")] Event @event, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                //Get UserID
                string UserId = User.Identity.GetUserId();

                //User not logged in, redirect to Login Page
                if (UserId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                //Set Event Owner ID equal to current user ID
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

            ViewBag.OID = @event.OwnerID;


            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventID,OwnerID,EventTitle,EventTown,EventVenue,EventAddress,EventDate,EventTime,EventDescription,EventCategory,EventYouTube,EventSoundCloud,EventFacebook,EventTwitter,EventInstagram,EventWebsite,EventTicketPrice,EventTicketStore,EventImage,EventPhoneNumber,EventEmail")] Event @event, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;

                @event = db.Events.Include(s => s.Files).SingleOrDefault(s => s.EventID == @event.EventID);

                //Image
                if (imageUpload != null && imageUpload.ContentLength > 0)
                {
                    if (@event.Files.Any(f => f.FileType == FileType.EventImage))
                    {
                        db.Files.Remove(@event.Files.First(f => f.FileType == FileType.EventImage));
                    }
                    var avatar = new File
                    {
                        FileName = System.IO.Path.GetFileName(imageUpload.FileName),
                        FileType = FileType.EventImage,
                        ContentType = imageUpload.ContentType
                    };
                    using (var reader = new System.IO.BinaryReader(imageUpload.InputStream))
                    {
                        avatar.Content = reader.ReadBytes(imageUpload.ContentLength);
                    }
                    @event.Files = new List<File> { avatar };
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
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
