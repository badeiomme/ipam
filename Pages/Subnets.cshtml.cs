using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ipam.Pages
{
    public class SubnetsModel : PageModel
    {
        public SubnetsModel(IPAMContext db)
        {
            Db = db;

            Subnets = Db.Data.Subnets;
            Hosts = Db.Data.Hosts;
        }

        [BindProperty]
        public NewSubnet NewSubnet { get; set; }

        public IPAMContext Db { get; }


        public List<Subnet> Subnets;
        public List<Host> Hosts;

        public IActionResult OnGet()
        {
            return Page();
        }

        public void OnPostAddSubnet()
        {
            if (ModelState.IsValid)
            {
                var subnet = new Subnet(NewSubnet.Address);
                var addr = subnet.GetNetworkAddress();

                Db.Data.Subnets.Add(new Subnet(addr, subnet.Size));

                Db.CalculateSubnetParents();
                Db.CalculateHostsSubnet();

                Db.SaveChanges();
            }
        }
    }

    public class NewSubnet
    {
        [Required]
        public String Address { get; set; }

    }
}