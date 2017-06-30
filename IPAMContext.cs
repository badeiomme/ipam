using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ipam
{
    public class Subnet
    {
        [JsonConstructor]
        public Subnet(String address, UInt16 size)
        {
            fill(address, size);
        }

        public Subnet(String cidr)
        {
            var tokens = cidr.Split("/");
            fill(tokens[0], Convert.ToUInt16(tokens[1]));
        }

        private void fill(String address, UInt16 size)
        {
            IPAddress addr;
            var ret = IPAddress.TryParse(address, out addr);

            if (!ret)
                throw new Exception();

            Size = size;
            Key = address.ToString() + "/" + size;

            ip = IP2Int(address);
            mask = ~(0xffffffff >> size);
            network = ip & mask;

            Address = IPAddress.Parse(Int2Str(network));
        }

        public String GetNetmask()
        {
            return Int2Str(mask);
        }

        public String GetNetworkAddress()
        {
            return Int2Str(network);
        }

        public String Key { get; private set; }
        public IPAddress Address { get; private set; }
        public UInt16 Size { get; private set; }

        public String Parent { get; set; }

        private uint ip, mask, network;

        private static uint IP2Int(string IPNumber)
        {
            uint ip = 0;
            string[] elements = IPNumber.Split(new Char[] { '.' });
            if (elements.Length == 4)
            {
                ip = Convert.ToUInt32(elements[0]) << 24;
                ip += Convert.ToUInt32(elements[1]) << 16;
                ip += Convert.ToUInt32(elements[2]) << 8;
                ip += Convert.ToUInt32(elements[3]);
            }
            return ip;
        }

        private static String Int2Str(uint ip)
        {
            var bytes = BitConverter.GetBytes(ip);
            return $"{bytes[3]}.{bytes[2]}.{bytes[1]}.{bytes[0]}";
        }
    }

    public class Host
    {
        public String Name { get; set; }
        public String Subnet { get; set; }
        public IPAddress IPAddress { get; set; }
        public String Owner { get; set; }
        public String Location { get; set; }
        public String Comment { get; set; }
        public String MacAddress { get; set; }
        public String Type { get; set; }
    }

    class IPAddressSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var str = reader.Value as String;
            IPAddress addr;
            var ret = IPAddress.TryParse(str, out addr);

            if (ret)
                return addr;

            return IPAddress.Parse("0.0.0.0");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    public class IPAMContext : DbContext
    {
        public IPAMContext()
        {
            String data = "";
            try
            {
                data = File.ReadAllText("data.bin");
            }
            catch
            {
            }

            try
            {
                Data = JsonConvert.DeserializeObject<Data>(data, new IPAddressSerializer());
            }
            catch (JsonReaderException ex)
            {
                throw ex;
            }

            if (Data == null)
                Data = new Data();
        }

        public override Int32 SaveChanges()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new IPAddressSerializer());
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter("data.bin"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, Data);
            }
            return 0;
        }

        public void CalculateSubnetParents()
        {
            foreach (var subnet in Data.Subnets)
            {
                subnet.Parent = null;
                var network = IPAddress.Parse(subnet.GetNetworkAddress());
                var mask = IPAddress.Parse(subnet.GetNetmask());
                
                var parent = Data.Subnets
                    .Where(s => s.Size < subnet.Size)
                    .Where(outer => outer.Address.IsInSameSubnet(network, IPAddress.Parse(outer.GetNetmask())))
                    .OrderByDescending(o => o.Size).FirstOrDefault();

                if (parent != null)
                {
                    subnet.Parent = parent.Key;
                }
            }

            SaveChanges();
        }
        public void CalculateHostsSubnet()
        {
            foreach (var host in Data.Hosts)
            {
                var subnet = Data.Subnets.Where(s => host.IPAddress.IsInSameSubnet(IPAddress.Parse(s.GetNetworkAddress()), IPAddress.Parse(s.GetNetmask())))
                    .OrderByDescending(ss => ss.Size).FirstOrDefault();

                host.Subnet = subnet?.Key;
            }
        }

        public Data Data { get; private set; }
        public List<String> Messages { get; } = new List<String>();
    }

    public class Data
    {
        public List<Subnet> Subnets { get; } = new List<Subnet>();
        public List<Host> Hosts { get; } = new List<Host>();
        public HashSet<String> Locations { get; } = new HashSet<String>();
        public HashSet<String> Owner { get; } = new HashSet<String>();
        public Dictionary<String, String> SubnetComments { get; } = new Dictionary<String, String>();
        public HashSet<String> DeviceTypes { get; } = new HashSet<String>();
    }
}