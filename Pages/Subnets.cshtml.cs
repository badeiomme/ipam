using System;
using System.Collections.Generic;
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
        public String NewAddress { get; set; }
        [BindProperty]
        public UInt16 NewSize { get; set; }

        public IPAMContext Db { get; }


        public List<Subnet> Subnets;
        public List<Host> Hosts;

        public IActionResult OnPost()
        {
            var subnet = new Subnet(NewAddress, NewSize);
            var addr = subnet.GetNetworkAddress();

            Db.Data.Subnets.Add(new Subnet(addr, NewSize));

            Db.CalculateSubnetParents();
            Db.CalculateHostsSubnet();

            Db.SaveChanges();

            return Page();
        }
        public IActionResult OnGet()
        {
            return Page();
        }
    }
}