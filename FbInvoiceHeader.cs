using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool
{
    internal class FbInvoiceHeader
    {
        public string fbInvoiceNumber {  get; set; }
        public string fbInvoiceDate { get; set; }
        public string fbInvoiceTime { get; set; }
        public double fbGrossSales { get; set; }
        public double fbTotalChargeAmount { get; set; }
        public double fbTotalTaxAmount { get; set; }
        public double fbDiscount {  get; set; }
        public double fbTotalSales {  get; set; }
        public string fbStatus { get; set; }
        public string fbMemberCode { get; set; }
        public string fbTimeStamp { get; set; }
        public string fbMysoftInv { get; set; }

    }
}
