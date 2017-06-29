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
    public class OwnerModel : PageModel
    {
        public OwnerModel(IPAMContext db)
        {
            Db = db;
        }

        public IPAMContext Db { get; }

        public void OnPost()
        {
        }


        public IActionResult OnPostAddOwner()
        {
            if (ModelState.IsValid)
            {
                Db.Data.Owner.Add(NewOwner.Owner);
                Db.SaveChanges();
            }
            return Page();
        }

        [BindProperty]
        public NewOwnerData NewOwner { get; set; }
    }

    public class NewOwnerData
    {
        [Required]
        public String Owner { get; set; }
    }
}