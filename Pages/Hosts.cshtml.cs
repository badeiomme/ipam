using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ipam.Pages
{
    public class HostsModel : PageModel
    {
        public HostsModel(IPAMContext db)
        {
            Db = db;
        }

        public IPAMContext Db { get; }

        [BindProperty]
        public NewHostData NewHostData { get; set; } = new NewHostData();

        public String Message { get; private set; }

        public IActionResult OnPost()
        {
            Message = NewHostData.Hostname;
            Message = "foo";
            return Page();
        }

        public List<SelectListItem> OwnerSelect
        {
            get
            {
                return Db.Data.Owner.Select(owner => new SelectListItem { Value = owner, Text = owner }).Prepend(new SelectListItem()).ToList();
            }
        }


        public List<SelectListItem> TypeSelect
        {
            get
            {
                return Db.Data.DeviceTypes.Select(type => new SelectListItem { Value = type, Text = type }).Prepend(new SelectListItem()).ToList();
            }
        }

        public List<SelectListItem> LocationSelect
        {
            get
            {
                return Db.Data.Locations.Select(location => new SelectListItem { Value = location, Text = location }).Prepend(new SelectListItem()).ToList();
            }
        }

        public IActionResult OnPostAddHost()
        {
            if (ModelState.IsValid)
            {
                var host = fillHost();

                if (host != null)
                {
                    if (Db.Data.Hosts.FirstOrDefault(h => h.Name == host.Name) != null)
                    {
                        Message = $"Host {host.Name} already added!";
                        return Page();
                    }

                    Db.Data.Hosts.Add(host);
                    Db.CalculateHostsSubnet();
                    
                    if (String.IsNullOrEmpty(host.Subnet))
                    {
                        Message = $"No Subnet found for IP {host.IPAddress}";
                        Db.Data.Hosts.Remove(host);

                        return Page();
                    }

                    Db.SaveChanges();
                }
            }

            return Page();
        }

        public IActionResult OnPostEditHost()
        {
            if (ModelState.IsValid)
            {
                var host = Db.Data.Hosts.First(h => h.Name == NewHostData.OldHostName);

                var ret = fillHost();

                if (ret != null)
                {
                    var idx = Db.Data.Hosts.FindIndex(h => h == host);
                    var old = Db.Data.Hosts[idx];
                    Db.Data.Hosts[idx] = ret;
                    Db.CalculateHostsSubnet();

                    if (String.IsNullOrEmpty(ret.Subnet))
                    {
                        Message = $"No Subnet found for IP {ret.IPAddress}";
                        Db.Data.Hosts[idx] = old;
                         return Page();
                    }

                    Db.SaveChanges();
                }
            }

            return Page();
        }

        private Host fillHost()
        {
            var host = new Host();
            host.Name = NewHostData.Hostname;

            IPAddress addr;
            var ret = IPAddress.TryParse(NewHostData.IPAddress, out addr);
            if (!ret)
            {
                Message = $"Invalid IP Address: {NewHostData.IPAddress}";
                return null;
            }
            else
                host.IPAddress = addr;

            if (!String.IsNullOrEmpty(NewHostData.Owner) && Db.Data.Owner.FirstOrDefault(o => o == NewHostData.Owner) == null)
            {
                Message = $"Owner {NewHostData.Owner} not found";
                return null;
            }
            else
                host.Owner = NewHostData.Owner;

            if (!String.IsNullOrEmpty(NewHostData.Location) && Db.Data.Locations.FirstOrDefault(o => o == NewHostData.Location) == null)
            {
                Message = $"Location {NewHostData.Location} not found";
                return null;
            }
            else
                host.Location = NewHostData.Location;

            host.Type = NewHostData.DeviceType;
            host.Comment = NewHostData.Comment;
            host.MacAddress = NewHostData.MacAddress;

            NewHostData = null;
            return host;
        }

    }
    public class NewHostData
    {
        [Required]
        [RegularExpression(@"[a-z][a-z0-9-]*")]
        public String Hostname { get; set; }

        [Required]
        [RegularExpression(@"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)")]
        public String IPAddress { get; set; }

        public String Owner { get; set; }

        public String Location { get; set; }

        public String Comment { get; set; }

        public String OldHostName { get; set; }

        [RegularExpression(@"[a-fA-F0-9]{2}(:[a-fA-F0-9]{2}){5}")]
        public String MacAddress { get; set; }

        [Required]
        public String DeviceType { get; set; }

    }
}