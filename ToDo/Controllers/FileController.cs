using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDo.Models;

namespace ToDo.Controllers
{
    public class FileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /File/
        public ActionResult Index(int id)
        {
            //Event File
            var fileToRetrieve = db.Files.Find(id);

            //Venue File
            var VfileToRetrieve = db.VenueFiles.Find(id);

            //Band File
            var BfileToRetrieve = db.BandFiles.Find(id);

            if (fileToRetrieve != null)
            {
                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            }

            else if (VfileToRetrieve != null)
            {
                return File(VfileToRetrieve.VenueContent, VfileToRetrieve.VenueContentType);
            }   
            
            else
                return File(BfileToRetrieve.BandContent, BfileToRetrieve.BandContentType);
        }
    }
}