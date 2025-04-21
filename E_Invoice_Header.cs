using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool
{
    internal class E_Invoice_Header
    {
        public string SupplierName { get; set; }
        public string SupplierTIN { get; set; }
        public string SupplierID { get; set; }
        public string SupplierSST { get; set; }
        public string SupplierTourismTax { get; set; }
        public string SupplierEmail { get; set; }
        public string SupplierMSIC { get; set; }
        public string SupplierBusinessActivityDesc { get; set; }
        public string SupplierAddress0 { get; set; }
        public string SupplierAddress1 { get; set; }
        public string SupplierAddress2 { get; set; }
        public string SupplierPostal { get; set; }
        public string SupplierCity { get; set; }
        public string SupplierState { get; set; }
        public string SupplierCountry { get; set; }
        public string SupplierContactNo { get; set; }
        public string BuyerName { get; set; }
        public string BuyerTIN { get; set; }
        public string BuyerID { get; set; }
        public string BuyerSST { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerAddress0 { get; set; }
        public string BuyerAddress1 { get; set; }
        public string BuyerAddress2 { get; set; }
        public string BuyerPostal { get; set; }
        public string BuyerCity { get; set; }
        public string BuyerState { get; set; }
        public string BuyerCountry { get; set; }
        public string BuyerContactNo { get; set; }
        public string H_E_InvoiceVersion { get; set; }
        public string H_E_InvoiceType { get; set; }
        public string H_E_InvoiceNumber { get; set; }
        public string H_E_InvoiceDate { get; set; }
        public string H_E_InvoiceTime { get; set; }
        public string H_SupplierDigitalSignature { get; set; }
        public string H_InvoiceCurCode { get; set; }
        public double H_ExchangeRate { get; set; }
        public string H_FrequencyBilling { get; set; }
        public string H_BillingPeriodStartDate { get; set; }
        public string H_BillingPeriodEndDate { get; set; }
        public string H_PaymentMode { get; set; }
        public string H_SupplierBankAC { get; set; }
        public string H_PaymentTerms { get; set; }
        public string H_PrePaymentAmount { get; set; }
        public string H_PrePaymentDate { get; set; }
        public string H_PrePaymentTime { get; set; }
        public string H_PrePaymentRefNo { get; set; }
        public string H_BillRefNo { get; set; }
        public double H_TotalExcludingTax { get; set; }
        public double H_TotalIncludingTax { get; set; }
        public double H_TotalPayableAmount { get; set; }
        public double H_TotalNetAmount { get; set; }
        public double H_TotalDiscountValue { get; set; }
        public double H_TotalChargeAmount { get; set; }
        public double H_TotalTaxAmount { get; set; }
        public double H_RoundingAmount { get; set; }
        public double H_TotalTaxableAmtPerTaxType { get; set; }
        public double H_TotalTaxAmtPerTaxType { get; set; }
        public string H_TaxType { get; set; }
        public double H_AdditionalDiscountAmount { get; set; }
        public double H_AdditionalFeeAmount { get; set; }
        public string H_ShippingName { get; set; }
        public string H_ShippingAddress0 { get; set; }
        public string H_ShippingAddress1 { get; set; }
        public string H_ShippingAddress2 { get; set; }
        public string H_ShippingPostal { get; set; }
        public string H_ShippingCity { get; set; }
        public string H_ShippingState { get; set; }
        public string H_ShippingCountry { get; set; }
        public string H_ShippingTIN { get; set; }
        public string H_ShippingID { get; set; }
        public string H_CustomForm1to9RefNo { get; set; }
        public string H_Incoterms { get; set; }
        public string H_FTAInfo { get; set; }
        public string H_CertifiedExporterAuthNo { get; set; }
        public string H_CustomForm2 { get; set; }
        public string H_OtherChargesDetail { get; set; }
        public string MySoft_ARAPCode { get; set; }
        public bool MySoft_IsSelfBill { get; set; }
        public string MySoft_UserName { get; set; }
        public DateTime MySoft_LogDate { get; set; }
        public string H_TaxExemption { get; set; }
        public string H_TaxExemptionAmount { get; set; }
        public Boolean eInv_isSubmit { get; set; }
        public string H_OriginalDocUUID { get; set; }
        public string H_OriginalDocNo { get; set; }
    }
}
