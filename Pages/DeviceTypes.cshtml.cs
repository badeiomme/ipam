
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
    public class DeviceModel : PageModel
    {
        public DeviceModel(IPAMContext db)
        {
            Db = db;
        }

        public IPAMContext Db { get; }

        public IActionResult OnPostAddDevice()
        {
            if (ModelState.IsValid)
            {
                Db.Data.DeviceTypes.Add(NewDevice.Device);
                Db.SaveChanges();
            }
            return Page();
        }

        [BindProperty]
        public NewDeviceData NewDevice { get; set; }
    }

    public class NewDeviceData
    {
        [Required]
        public String Device { get; set; }
    }
}