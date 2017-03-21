using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ToDo.Models;

namespace ToDo.Controllers
{
    public class BandsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bands
        public ActionResult Index()
        {

            //Get UserID
            string UserId = User.Identity.GetUserId();

            //Check if the user is logged in
            if (UserId != null)
            {
                ViewBag.LoggedIn = true;
            }

            ViewBag.LinkText = "Bands";

            //See BandsTablePartialView
            return View();
        }

        //Venue Index Partial View
        public ActionResult BandsTablePartialView(string search, string BandGenre, string AdvancedSearch, string GenreIndex)
        {
            ViewBag.GenreIndex = GenreIndex;

            ViewBag.Genres = new SelectList(db.MusicGenres.OrderBy(x => x.MusicGenreName), "MusicGenreID", "MusicGenreName");

            //Get all Bands from the database
            var bands = from b in db.Bands
                        where b.BandActive == true
                        select b;

            if (AdvancedSearch == "true")
            {

                bands = from b in db.Bands
                        where b.BandActive == true
                        select b;

                if (BandGenre != "")
                {
                    int genreID = Convert.ToInt32(BandGenre);
                    bands = bands.Where(b => b.BandGenre.MusicGenreID == genreID);
                }

                if (!String.IsNullOrEmpty(search))
                {
                    //Get all the bands where the name contains the users search term
                    bands = bands.Where(e => e.BandName.ToUpper().Contains(search.ToUpper()));
                    ViewBag.SearchTerm = search;
                }

                if (bands.Count() <= 0)
                {
                    ViewBag.NoBands = true;
                }

                else
                {
                    ViewBag.NoBands = false;
                }

                return PartialView("_BandsTable", bands.OrderBy(e => e.BandName).ToList());
            }

            else
            {
                //Search Bar
                if (!String.IsNullOrEmpty(search))
                {
                    //Get all the events where the name contains the users search term
                    bands = bands.Where(e => e.BandName.ToUpper().Contains(search.ToUpper()));
                    ViewBag.SearchTerm = search;
                }

                if (bands.Count() <= 0)
                {
                    ViewBag.NoBands = true;
                }

                else
                {
                    ViewBag.NoBands = false;
                }

                return PartialView("_BandsTable", bands.OrderBy(v => v.BandName).ToList());
            }
        }

        // GET: Bands/Details/5
        public ActionResult Details(int? id)
        {

            ViewBag.LinkText = "Bands";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Band band = db.Bands.Find(id);

            if (band == null)
            {
                return HttpNotFound();
            }

            //Get UserID
            string UserId = User.Identity.GetUserId();

            //Check if current user is the owner of this event
            if (band.OwnerId == UserId)
            {
                ViewBag.IsOwner = true;
            }

            //YouTube
            if (band.BandYouTube != null)
            {
                ViewBag.hasYT = true;
                ViewBag.youTubeID = band.BandYouTube.Substring(band.BandYouTube.LastIndexOf('=') + 1);
            }

            else
            {
                ViewBag.hasYT = false;
            }

            //Soundcloud
            if (band.BandSoundCloud != null)
            {
                ViewBag.hasSC = true;
                ViewBag.SoundCloud = band.BandSoundCloud;
            }

            else
            {
                ViewBag.hasSC = false;
            }

            //Get Events for this band
            var events = (from e in db.Events
                          where e.BandID == id && e.EventActive == true
                          select e).OrderBy(x => x.EventDate).ToList();

            band.BandEvents = events;

            //Get list of events for this band
            if (band.BandEvents.Count() > 0)
            {
                ViewBag.hasEvents = true;
            }
            else
            {
                ViewBag.hasEvents = false;
            }

            //Sort Events
            foreach (var Event in band.BandEvents.ToList())
            {
                var EventDate = Event.EventDate;

                TimeSpan ts = DateTime.Now - EventDate;

                //Remove event if it is old
                if (ts.TotalDays > 1)
                {
                    db.Events.Remove(Event);
                    db.SaveChanges();
                }
            }

            return View(band);
        }

        public ActionResult BookBand(int? id)
        {

            //Get UserID
            string UserId = User.Identity.GetUserId();

            //User not logged in, redirect to Login Page
            if (UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Get Venues that the user created
            var venues = (from v in db.Venues
                          where v.OwnerId == UserId
                          select v).ToList();

            if (venues.Count <= 0)
            {
                ViewBag.NoVenues = true;
                ViewBag.Messgae = "You do not currently have any venues to book this band with, please register your venue first";
            }

            else
            {
                ViewBag.NoVenues = false;

                //Store the Bands ID
                TempData["BandID"] = id;

                //User Venues
                ViewBag.VenuesID = new SelectList(venues.OrderBy(x => x.VenueName), "VenueID", "VenueName");

                //Event Categroies
                ViewBag.EventCategories = new SelectList(db.EventCategories.OrderBy(x => x.EventCategoryName), "EventCategoryID", "EventCategoryName");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookBand([Bind(Include = "EventID, VenueID, EventTitle, EventDate, EventTime, EventEndTime, EventDescription, EventYouTube, EventSoundCloud, EventFacebook, EventTwitter, EventInstagram, EventWebsite, EventTicketPrice, EventTicketStore, EventCatID")] Event @event, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                //Get the currentely logged in user
                string UserId = User.Identity.GetUserId();

                //If no user is logged in, redirect them to the login page
                if (UserId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                //Get Owner Id
                @event.OwnerID = UserId;

                //Get the new id for this event
                var NextId = this.db.Events.Max(t => t.EventID);
                var newId = NextId + 1;
                @event.EventID = newId;

                //Set this Events status as active
                @event.EventActive = true;

                //Event Category
                @event.EventCat = db.EventCategories.Find(@event.EventCatID);

                //View Counter
                @event.EventViewCounterReset = DateTime.Now.Date;
                @event.EventDailyViewCounter = 0;
                @event.EventViewCounter = 0;

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

                //Band ID
                int BandID = Convert.ToInt32(TempData["BandID"]);
                @event.BandID = BandID;               


                db.Events.Add(@event);
                db.SaveChanges();



                //Redirect to details view for the new event
                return RedirectToAction("Details", "Events", new
                {
                    id = @event.EventID
                });
            }

            ViewBag.VenueID = new SelectList(db.Venues, "VenueID", "OwnerId", @event.VenueID);
            return View(@event);

        }

        // GET: Bands/Create
        public ActionResult Create()
        {

            ViewBag.LinkText = "Bands";

            //Get the currentely logged in user
            string currentUserId = User.Identity.GetUserId();

            //If no user is logged in, redirect them to the login page
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Genre
            ViewBag.Genres = new SelectList(db.MusicGenres.OrderBy(x => x.MusicGenreName), "MusicGenreID", "MusicGenreName");

            return View();
        }

        // POST: Bands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BandID,BandName,BandDescription,BandContactNumber,BandEmail,BandFacebook,BandGenreID,BandYouTube,BandSoundCloud,BandManagerName,BandManagerEmail,BandPressContact,BandRecordLabel,BandBookingAgentName,BandBookingAgentEmail")] Band band, HttpPostedFileBase imageUpload)
        {

            //Get the currentely logged in user
            string UserId = User.Identity.GetUserId();

            //If no user is logged in, redirect them to the login page
            if (UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Band Genre
            band.BandGenre = db.MusicGenres.Find(band.BandGenreID);
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            //Set the band OwnerID equal to the current user ID
            band.OwnerId = UserId;

            int x = band.BandID;

            if (ModelState.IsValid)
            {

                //Set the band status as active
                band.BandActive = true;

                //Image File Upload
                if (imageUpload != null && imageUpload.ContentLength > 0)
                {
                    var imgBand = new BandFile
                    {
                        BandFileName = System.IO.Path.GetFileName(imageUpload.FileName),
                        BandFileType = FileType.EventImage,
                        BandContentType = imageUpload.ContentType
                    };
                    using (var reader = new System.IO.BinaryReader(imageUpload.InputStream))
                    {
                        imgBand.BandContent = reader.ReadBytes(imageUpload.ContentLength);
                    }

                    band.BandFiles = new List<BandFile> { imgBand };
                }

                db.Bands.Add(band);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(band);
        }

        // GET: Bands/Edit/5
        public ActionResult Edit(int? id)
        {

            ViewBag.LinkText = "Bands";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Band band = db.Bands.Find(id);

            if (band == null)
            {
                return HttpNotFound();
            }


            //Image
            band = db.Bands.Include(s => s.BandFiles).SingleOrDefault(s => s.BandID == id);

            //Owner ID
            ViewBag.OID = band.BandID;

            //Event Catagories
            ViewBag.MusicGenres = new SelectList(db.MusicGenres.OrderBy(x => x.MusicGenreName), "MusicGenreID", "MusicGenreName");
            int musicGenreID = band.BandGenreID;

            return View(band);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, HttpPostedFileBase upload)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var BandToUpdate = db.Bands.Find(id);
            if (TryUpdateModel(BandToUpdate, "",

                new string[] { "BandID", "BandName", "BandDescription", "BandContactNumber", "BandEmail", "BandFacebook", "BandGenreID", "BandYouTube", "BandSoundCloud", "BandManagerName", "BandManagerEmail", "BandPressContact", "BandRecordLabel", "BandBookingAgentName", "BandBookingAgentEmail" }))
            {
                try
                {
                    if (upload != null && upload.ContentLength > 0)
                    {
                        if (BandToUpdate.BandFiles.Any(f => f.BandFileType == FileType.EventImage))
                        {
                            db.BandFiles.Remove(BandToUpdate.BandFiles.First(f => f.BandFileType == FileType.EventImage));
                        }
                        var img = new BandFile
                        {
                            BandFileName = System.IO.Path.GetFileName(upload.FileName),
                            BandFileType = FileType.EventImage,
                            BandContentType = upload.ContentType
                        };
                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            img.BandContent = reader.ReadBytes(upload.ContentLength);
                        }
                        BandToUpdate.BandFiles = new List<BandFile> { img };
                    }

                    Band oldBand = db.Bands.Find(BandToUpdate.BandID);

                    BandToUpdate.BandFiles = oldBand.BandFiles;

                    db.Entry(BandToUpdate).State = EntityState.Modified;
                    db.SaveChanges();

                    //Event Category
                    Band bandA = db.Bands.Find(BandToUpdate.BandID);
                    bandA.BandGenre = db.MusicGenres.Find(BandToUpdate.BandGenreID);

                    db.SaveChanges();



                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(BandToUpdate);
        }

        // POST: Bands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "BandID,BandName,BandDescription,BandContactNumber,BandEmail,BandFacebook,BandGenreID,BandYouTube,BandSoundCloud,BandManagerName,BandManagerEmail,BandPressContact,BandRecordLabel,BandBookingAgentName,BandBookingAgentEmail")] Band band)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(band).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(band);
        //}

        // GET: Bands/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Band band = db.Bands.Find(id);
            if (band == null)
            {
                return HttpNotFound();
            }
            return View(band);
        }

        // POST: Bands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Band band = db.Bands.Find(id);
            db.Bands.Remove(band);
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

        // Remove Venue 
        public ActionResult RemoveBand(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Band band = db.Bands.Find(id);

            if (band == null)
            {
                return HttpNotFound();
            }

            //Set this event as inactive
            band.BandActive = false;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //Band Events Partial View
        public ActionResult BandEventsPartialView(int? BandID, string search, string CategoryIndex, string EventCategory, DateTime? Date)
        {
            ViewBag.BandID = BandID;
            ViewBag.CategoryIndex = CategoryIndex;

            //Get Events
            var events = (from e in db.Events
                          where e.BandID == BandID && e.EventActive == true
                          select e).ToList();

            //Event Categories
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

            if (events.Count() <= 0)
            {
                ViewBag.NoEvents = true;
            }

            else
            {
                ViewBag.NoEvents = false;
            }


            return PartialView("_BandEvents", events.OrderBy(v => v.EventTitle).ToList());
        }
    }
}
