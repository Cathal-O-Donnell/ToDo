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

namespace ToDo.Controllers
{
    public class VenuesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Venues
        public ActionResult Index()
        {
            return View(db.Venues.ToList());
        }

        // GET: Venues/Details/5
        public ActionResult Details(int? id)
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

            //Get UserID
            string UserId = User.Identity.GetUserId();

            //Check if current user is the owner of this event
            if (venue.OwnerId == UserId)
            {
                ViewBag.IsOwner = true;
            }

            //Get Events
            var events = (from e in db.Events
                      where e.VenueID == id
                      select e).ToList();

            venue.VenueEvents = events;

            return View(venue);
        }

        // GET: Venues/Create
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

        // POST: Venues/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VenueID,VenueName,VenueType,VenueTown,VenueAddress,VenueDescription,VenueEmail,VenuePhoneNumber")] Venue venue, HttpPostedFileBase imageUpload)
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

                venue.OwnerId = UserId;

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

                    venue.Files = new List<File> { imgEvent };
                }

                db.Venues.Add(venue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(venue);
        }

        // GET: Venues/Edit/5
        public ActionResult Edit(int? id)
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

        // POST: Venues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VenueID,OwnerId,VenueName,VenueType,EventTown,EventAddress,EventDescription,EventEmail,EventPhoneNumber")] Venue venue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(venue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(venue);
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
            db.Venues.Remove(venue);
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
