using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDo.Models;

namespace ToDo.Controllers
{
    public class VenueFileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /File/
        public ActionResult Index(int id)
        {
            //Venue File
            var VfileToRetrieve = db.VenueFiles.Find(id);


            if (VfileToRetrieve != null)
            {
                return File(VfileToRetrieve.VenueContent, VfileToRetrieve.VenueContentType);
            }

            else
            {
                return null;
            }
        }
    }
}