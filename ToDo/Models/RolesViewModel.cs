using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ToDo.Models
{
    public class RolesViewModel
    {
        [Display(Name = "Role")]
        public IEnumerable<string> RoleNames { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }

        [Display(Name = "UserId")]
        public string Id { get; set; }
    }
}