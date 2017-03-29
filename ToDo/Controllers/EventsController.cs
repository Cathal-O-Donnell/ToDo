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
using System.Globalization;

namespace ToDo.Controllers
{
    [RequireHttps]
    public class EventsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Events
        public ActionResult Index()
        {

            ViewBag.LinkText = "Events";

            //See EventsTablePartialView
            return View();
        }

        // GET: Events/Details/5
        public ActionResult Details(int? id)
        {

            ViewBag.LinkText = "Events";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Get selected event from DB
            Event @event = db.Events.Find(id);

            if (@event == null)
            {
                return HttpNotFound();
            }

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

            //Get current users id
            string UserId = User.Identity.GetUserId();

            //Check if current user is the owner of this event
            if (@event.OwnerID == UserId)
            {
                ViewBag.IsOwner = true;

                //Reset the daily view counter
                if (DateTime.Now.Date != @event.EventViewCounterReset)
                {
                    @event.EventViewCounterReset = DateTime.Now.Date;
                    @event.EventDailyViewCounter = 0;
                }
            }

            else
            {
                ViewBag.IsOwner = false;

                //If user is not owner of this event, add 1 to the view counter
                @event.EventViewCounter = @event.EventViewCounter +1;

                //Reset the daily view counter
                if (DateTime.Now.Date != @event.EventViewCounterReset)
                {
                    @event.EventViewCounterReset = DateTime.Now.Date;
                    @event.EventDailyViewCounter = 0;
                }

                //Incremente the daily view counter
                @event.EventDailyViewCounter = @event.EventDailyViewCounter + 1;

                db.SaveChanges();
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

            //Get Event Image
            @event = db.Events.Include(s => s.Files).SingleOrDefault(s => s.EventID == id);

            //Facebook
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
                        
            return View(@event);
        }

        // GET: Events/Create
        public ActionResult Create(int? id)
        {

            ViewBag.LinkText = "Events";

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

            //Event Categories
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

            ViewBag.LinkText = "Events";

            if (ModelState.IsValid)
            {
                Venue venue = db.Venues.Find(@event.VenueID);
                string venueName = venue.VenueName;
                string eventDes = @event.EventDescription;
                string eventDate = Convert.ToString(@event.EventDate.Date.ToShortDateString());
                string eventTime = Convert.ToString(@event.EventTime.TimeOfDay);

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

                db.Events.Add(@event);
                db.SaveChanges();

                string emailSubject = @event.EventTitle;
                string emailLink = string.Format("<a href = \"https://localhost:44300/Events/Details/{0}\"> here </a>", @event.EventID);
                string emailBody = string.Format("New event posted for <b>{0}</b> on {2} {3}.<br><br> {1} <br><br> {4} <a>", venueName, eventDes, eventDate, eventTime, emailLink );

                EmailNotification(@event.VenueID, emailSubject, emailBody);
                
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

            ViewBag.LinkText = "Events";

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

        // Remove Venue 
        public ActionResult DeleteEvent(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Event @event = db.Events.Find(id);
            int venueID = @event.VenueID;            

            if (@event == null)
            {
                return HttpNotFound();
            }

            Venue venue = db.Venues.Find(@event.VenueID);
            string venueName = venue.VenueName;
            string eventTitle = @event.EventTitle;

            db.Events.Remove(@event);
            db.SaveChanges();

            

            //Email Notification
            string emailSubject = string.Format(@event.EventTitle + " Cancelled");
            string emailBody = string.Format("{0} has cencelled the event: {1}", venueName, eventTitle);
            EmailNotification(@event.VenueID, emailSubject, emailBody);

            //Redirect to details view for the current venue
            return RedirectToAction("Details", "Venues", new
            {
                id = venueID
            });
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
            

            //Set this event as inactive
            @event.EventActive = false;

            Venue venue = db.Venues.Find(@event.VenueID);
            string venueName = venue.VenueName;
            string eventDes = @event.EventDescription;
            string eventDate = Convert.ToString(@event.EventDate.Date.ToShortDateString());
            string eventTime = Convert.ToString(@event.EventTime.TimeOfDay);

            string emailSubject = string.Format(@event.EventTitle + "cancelled");
            string emailBody = string.Format("This event has been cancelled");


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


        //Event Index Partial View
        public ActionResult EventsTablePartialView(string search, string Town, string EventCategory, string AdvancedSearch, string TownIndex, string CategoryIndex, DateTime? Date)
        {          
                                  
            ViewBag.TownIndex = TownIndex;
            ViewBag.CategoryIndex = CategoryIndex;

            ViewBag.Towns = new SelectList(db.Towns.OrderBy(x => x.TownName), "TownId", "TownName");
            ViewBag.EventCategories = new SelectList(db.EventCategories.OrderBy(x => x.EventCategoryName), "EventCategoryID", "EventCategoryName");

            //Get all events from the database
            var events = from e in db.Events
                         where e.EventActive == true && e.Venue.VenueActive == true
                         select e;

            //Remove old events from the database
            foreach (var Event in events.ToList())
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

                if (AdvancedSearch == "true")
            {               
                //Get all Events from the database
                events = from v in db.Events
                         where v.EventActive == true
                         select v;

                if (Date != null)
                {
                    var stringDate = String.Format("{0:dd-MM-yyyy}", Date);
                    DateTime formatedDate = DateTime.Parse(stringDate);
                    events = events.Where(e => e.EventDate == formatedDate);
                }

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

                if (events.Count() <= 0)
                {
                    ViewBag.NoEvents = true;
                }

                else
                {
                    ViewBag.NoEvents = false;
                }

                return PartialView("_EventsTable", events.OrderBy(e => e.EventDate).ToList());
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

                if (events.Count() <= 0)
                {
                    ViewBag.NoEvents = true;
                }

                else
                {
                    ViewBag.NoEvents = false;
                }

                return PartialView("_EventsTable", events.OrderBy(v => v.EventDate).ToList());
            }
        }

        public void EmailNotification(int id, string Subject, string Body)
        {
            //Get Subscribers List
            List<string> subscribers = (from e in db.VenueMailingList
                                        where e.VenueID == id
                                        select e.UserEmail).ToList();

            //Send an email to every subscriber to this venue
            foreach (var email in subscribers)
            {
                GMailer.GmailUsername = "ToDoEventsGuide@gmail.com";
                GMailer.GmailPassword = "todosoftware";

                GMailer mailer = new GMailer();
                mailer.ToEmail = email;
                mailer.Subject = Subject;
                mailer.Body = Body;
                mailer.IsHtml = true;
                mailer.Send();
                
            }
        }
    }
}

//Tutorial used: http://stackoverflow.com/questions/20882891/how-can-i-send-email-using-gmail-smtp-in-asp-net-mvc-application