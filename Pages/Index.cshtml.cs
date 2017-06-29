using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ipam.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IPAMContext db)
        {
            Db = db;
        }

        private IPAMContext Db { get; }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Subnets");
        }
    }
}
