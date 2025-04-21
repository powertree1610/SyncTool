using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool
{
    internal class FbInvoiceDetail
    {
        public int lineNo {  get; set; }
        public string productDescription { get; set; }
        public double unitPrice { get; set; }
        public double taxRate { get; set; }
        public double taxAmount { get; set; }
        public double subtotal { get; set; }
        public double discount { get; set; }
        public double quantity { get; set; }
        public double chargeRate { get; set; }
        public double chargeAmount { get; set; }
        public string CRemark { get; set; }
        public string unitOfMeasurement { get; set; }
        public string productCode { get; set; }
    }
}
