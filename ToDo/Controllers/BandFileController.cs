using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDo.Models;

namespace ToDo.Controllers
{
    public class BandFileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /File/
        public ActionResult Index(int id)
        {
            //Band File
            var BfileToRetrieve = db.BandFiles.Find(id);


            if (BfileToRetrieve != null)
            {
                return File(BfileToRetrieve.BandContent, BfileToRetrieve.BandContentType);
            }

            else
            {
                return null;
            }
        }
    }
}