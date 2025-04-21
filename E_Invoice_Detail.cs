using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool
{
    internal class E_Invoice_Detail
    {
        public string H_E_InvoiceNumber { get; set; }
        public string SupplierTIN { get; set; }
        public string BuyerTIN { get; set; }
        public string LI_ProductClassification { get; set; }
        public string LI_ProductDescription { get; set; }
        public double LI_UnitPrice { get; set; }
        public string LI_TaxType { get; set; }
        public double LI_TaxRate { get; set; }
        public double LI_TaxAmount { get; set; }
        public string LI_TaxExemption { get; set; }
        public double LI_TaxExemptionAmount { get; set; }
        public double LI_SubTotal { get; set; }
        public double LI_TotalExcludingTax { get; set; }
        public double LI_Quantity { get; set; }
        public string LI_UOM { get; set; }
        public double LI_DiscountRate { get; set; }
        public double LI_DiscountAmount { get; set; }
        public double LI_ChargeRate { get; set; }
        public double LI_ChargeAmount { get; set; }
        public string LI_ProductTariffCode { get; set; }
        public string LI_CountryOrigin { get; set; }
        public string MySoft_StockCode { get; set; }
        public int MySoft_LineNo { get; set; }
    }
}
