using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ToDo.Models
{
    public class SelectListModel
    {
        public int AdminSettingsID { get; set; }

        //Top Event
        public int TopEventId { get; set; }
        [Display(Name = "Top Event")]
        public IEnumerable<SelectListItem> TopEventOption { get; set; }

        //Featured Event1
        public int Event1Id { get; set; }
        [Display(Name = "Featured Event 1")]
        public IEnumerable<SelectListItem> Event1_Option { get; set; }

        //Featured Event2
        public int Event2Id { get; set; }
        [Display(Name = "Featured Event 2")]
        public IEnumerable<SelectListItem> Event2_Option { get; set; }

        //Featured Event3
        public int Event3Id { get; set; }
        [Display(Name = "Featured Event 3")]
        public IEnumerable<SelectListItem> Event3_Option { get; set; }

        //Featured Event4
        public int Event4Id { get; set; }
        [Display(Name = "Featured Event 4")]
        public IEnumerable<SelectListItem> Event4_Option { get; set; }

        //Top Venue
        public int TopVenueId { get; set; }
        [Display(Name = "Top Venue")]
        public IEnumerable<SelectListItem> TopVenueOption { get; set; }

        //Featured Venue1
        public int Venue1Id { get; set; }
        [Display(Name = "Featured Venue 1")]
        public IEnumerable<SelectListItem> Venue1_Option { get; set; }

        //Featured Venue2
        public int Venue2Id { get; set; }
        [Display(Name = "Featured Venue 2")]
        public IEnumerable<SelectListItem> Venue2_Option { get; set; }

        //Featured Venue3
        public int Venue3Id { get; set; }
        [Display(Name = "Featured Venue 3")]
        public IEnumerable<SelectListItem> Venue3_Option { get; set; }

        //Featured Venue4
        public int Venue4Id { get; set; }
        [Display(Name = "Featured Venue 4")]
        public IEnumerable<SelectListItem> Venue4_Option { get; set; }
    }
}