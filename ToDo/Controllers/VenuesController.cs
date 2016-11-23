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
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "VenueID,OwnerId,VenueName,VenueType,VenueTown,VenueAddress,VenueDescription,VenueEmail,VenuePhoneNumber")] Venue venue, HttpPostedFileBase upload)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ////New Image
        //        //if (upload != null && upload.ContentLength > 0)
        //        //{
        //        //    //Get Old Image
        //        //    var oldImage = (from e in db.VenueFiles
        //        //                  where e.VenueID == venue.VenueID
        //        //                  select e).ToList();

        //        //    venue.VenueFiles = oldImage;

        //        //    if (venue.VenueFiles != null)
        //        //    {
        //        //        if (venue.VenueFiles.Any(f => f.VenueFileType == FileType.EventImage))
        //        //        {
        //        //            db.VenueFiles.Remove(venue.VenueFiles.First(f => f.VenueFileType == FileType.EventImage));
        //        //        }
        //        //    }

        //        //    var newImg = new VenueFile
        //        //    {
        //        //        VenueFileName = System.IO.Path.GetFileName(upload.FileName),
        //        //        VenueFileType = FileType.EventImage,
        //        //        VenueContentType = upload.ContentType
        //        //    };
        //        //    using (var reader = new System.IO.BinaryReader(upload.InputStream))
        //        //    {
        //        //        newImg.VenueContent = reader.ReadBytes(upload.ContentLength);
        //        //    }

        //        //    venue.VenueFiles = new List<VenueFile> { newImg };

        //        //    /*venue.VenueFiles.Add(newImg);*/
        //        //}

        //        if (upload != null && upload.ContentLength > 0)
        //        {
        //            if (venue.VenueFiles.Any(f => f.VenueFileType == FileType.EventImage))
        //            {
        //                db.VenueFiles.Remove(venue.VenueFiles.First(f => f.VenueFileType == FileType.EventImage));
        //            }
        //            var avatar = new VenueFile
        //            {
        //                VenueFileName = System.IO.Path.GetFileName(upload.FileName),
        //                VenueFileType = FileType.EventImage,
        //                VenueContentType = upload.ContentType
        //            };
        //            using (var reader = new System.IO.BinaryReader(upload.InputStream))
        //            {
        //                avatar.VenueContent = reader.ReadBytes(upload.ContentLength);
        //            }
        //            venue.VenueFiles = new List<VenueFile> { avatar };
        //        }
        //        db.Entry(venue).State = EntityState.Modified;
        //        db.SaveChanges();

        //        db.Entry(venue).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(venue);
        //}

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
                new string[] { "VenueID","OwnerId","VenueName","VenueType","VenueTown","VenueAddress","VenueDescription","VenueEmail","VenuePhoneNumber" }))
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

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(venueToUpdate);
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
        public ActionResult VenuesTablePartialView(string search)
        {
            
            var venues = from v in db.Venues
                         select v;

            //Search Bar
            if (!String.IsNullOrEmpty(search))
            {
                venues = venues.Where(v => v.VenueName.ToUpper().Contains(search.ToUpper()));
            }

            //string sortOrder,
            //Filter 
            //ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";

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

            return PartialView("_VenuesTable", venues.OrderBy(v => v.VenueName).ToList());
        }
    }
}
