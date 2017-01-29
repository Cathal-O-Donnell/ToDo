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
                    bands = bands.Where(b =>b.BandGenre.MusicGenreID == genreID);
                }

                if (!String.IsNullOrEmpty(search))
                {
                    //Get all the bands where the name contains the users search term
                    bands = bands.Where(e => e.BandName.ToUpper().Contains(search.ToUpper()));
                    ViewBag.SearchTerm = search;
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

                return PartialView("_BandsTable", bands.OrderBy(v => v.BandName).ToList());
            }
        }

        // GET: Bands/Details/5
        public ActionResult Details(int? id)
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

            return View(band);
        }

        // GET: Bands/Create
        public ActionResult Create()
        {

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
            //Band Genre
            band.BandGenre = db.MusicGenres.Find(band.BandGenreID);
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            //Get the currentely logged in user
            string UserId = User.Identity.GetUserId();

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
    }
}
