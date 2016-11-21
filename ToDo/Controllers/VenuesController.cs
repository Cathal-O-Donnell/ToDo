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
            //See VenuesTablePartialView
            return View();
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

            //Check if there is any events for this Venue
            if (events.Count() < 1)
            {
                ViewBag.noEvents = true;
            }

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

            //Image
            venue = db.Venues.Include(s => s.VenueFiles).SingleOrDefault(s => s.VenueID == id);

            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VenueID,OwnerId,VenueName,VenueType,VenueTown,VenueAddress,VenueDescription,VenueEmail,VenuePhoneNumber")] Venue venue, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                //New Image
                if (upload != null && upload.ContentLength > 0)
                {
                    if (venue.VenueFiles.Any(f => f.VenueFileType == FileType.EventImage))
                    {
                        db.VenueFiles.Remove(venue.VenueFiles.First(f => f.VenueFileType == FileType.EventImage));
                    }
                    var avatar = new VenueFile
                    {
                        VenueFileName = System.IO.Path.GetFileName(upload.FileName),
                        VenueFileType = FileType.EventImage,
                        VenueContentType = upload.ContentType
                    };
                    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                    {
                        avatar.VenueContent = reader.ReadBytes(upload.ContentLength);
                    }
                    venue.VenueFiles = new List<VenueFile> { avatar };
                }

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

        //Venue Index Partial View
        //[ChildActionOnly]
        public ActionResult VenuesTablePartialView(string id)
        {
            //string sortOrder,
            //Filter 
            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";

            var venues = from v in db.Venues
                         select v;

            //Search Bar
            if (!String.IsNullOrEmpty(id))
            {
                venues = venues.Where(v => v.VenueName.ToUpper().Contains(id.ToUpper()));
            }

            //Sort Order
            //switch (sortOrder)
            //{
            //    case "Name_desc":
            //        venues = venues.OrderByDescending(v => v.VenueName);
            //        break;

            //    case "Name_assc":
            //        venues = venues.OrderBy(v => v.VenueName);
            //        break;

            //    default:
            //        venues = venues.OrderBy(v => v.VenueName);
            //        break;
            //}

            // return View(venues.ToList());
            return PartialView("_VenuesTable", venues.ToList());
        }
    }
}
