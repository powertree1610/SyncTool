using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool
{
    internal class FbBuyerInfo
    {
        public string Name { get; set; }
        public string IdNumber { get; set; }
        public string TinNumber { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Email { get; set; }
        public string Postal { get; set; }
        public string CityName { get; set; }
        public string State {  get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string SST { get; set; }
    }
}
