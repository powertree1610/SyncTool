using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool
{
    internal class Customer
    {
        public string BuyerCode { get; set; }
        public string BuyerName { get; set; }
        public string BuyerTin { get; set; }
        public string BuyerIDType { get; set; }
        public string BuyerID { get; set; }
        public string BuyerSST { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerAddress0 { get; set; }
        public string BuyerAddress1 { get; set; }
        public string BuyerAddress2 { get; set; }
        public string BuyerPostal { get; set; }
        //Default -
        public string BuyerCity { get; set; }
        //Default 00
        public string BuyerState { get; set; }
        //Default to MYS
        public string BuyerCountry { get; set; }
        public string BuyerContactNo { get; set; }
    }
}
