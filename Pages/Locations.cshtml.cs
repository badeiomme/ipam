using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ipam.Pages
{
    public class LocationModel : PageModel
    {
        public LocationModel(IPAMContext db)
        {
            Db = db;
        }

        public IPAMContext Db { get; }

        public IActionResult OnPostAddLocation()
        {
            if (ModelState.IsValid)
            {
                Db.Data.Locations.Add(NewLocation.Location);
                Db.SaveChanges();
            }
            return Page();
        }

        [BindProperty]
        public NewLocationData NewLocation { get; set; }
    }

    public class NewLocationData
    {
        [Required]
        public String Location { get; set; }
    }
}