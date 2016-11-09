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

            if (fileToRetrieve != null)
            {
                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            }

            else
            {
               return File(VfileToRetrieve.VenueContent, VfileToRetrieve.VenueContentType);
            }            
        }
    }
}