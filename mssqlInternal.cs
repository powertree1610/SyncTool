using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace SyncTool
{


    internal class mssqlInternal
    {
        //private string mssqlConnectionString = "Server=POSSERVER\\SQLEXPRESS;Database=AccDatabase1;User Id=sa;Password=Sql1234;";
        public string mssqlConnectionString;

        // Initial the Connection String 
        public mssqlInternal()
        {
            mssqlConnectionString = SQLConnectionStringFromJson();
        }

        // Get the Connection String from JSON
        private string SQLConnectionStringFromJson()
        {
            string jsonFilePath = "ConnectionString.json"; // or "dbsettings.json" if you used a different name
            string connectionStringName = "MSSQLConnection";

            try
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                JsonDocument doc = JsonDocument.Parse(jsonString);

                string connectionString = doc.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty(connectionStringName)
                    .GetString();

                LogMessage($"Connection string read successfully: {connectionString}");
                return connectionString;
            }
            catch (Exception ex)
            {
                LogMessage($"Error reading connection string: {ex.Message}");
                return null;
            }
        }

        // Any ERROR will print the message in the log file 
        static void LogMessage(string message)
        {
            // Ensure log folder exists
            string logFolder = "log";
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            // Generate the log file name with the current date
            string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
            string logFilePath = Path.Combine(logFolder, $"{todayDate}_log.txt");

            try
            {
                // Append new message to today's log file
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"[{DateTime.Now}] {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while logging message: {ex.Message}");
            }
        }

        // Get Current TimeStamp in mssql
        public DateTime checkTimeStamp()
        {
            string mssqlCheckTimeQuery = "SELECT MAX(newTimeStamp) FROM SyncTime";
            try
            {
                using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand mssqlCheckCommand = new SqlCommand(mssqlCheckTimeQuery, mssqlConnection))
                    {
                        mssqlConnection.Open();
                        object result = mssqlCheckCommand.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            mssqlConnection.Close();

                            return (DateTime)result;
                        }
                        else
                        {
                            mssqlConnection.Close();

                            return new DateTime(2000, 1, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<checkInoviceTime> Error : {ex.Message} ");
                return new DateTime(2000, 1, 1);
            }

        }

        // Update TimeStamp in mssql 
        public void updateTimestamp(DateTime? dt)
        {
            string checkRecordExistsQuery = "SELECT COUNT(1) FROM SyncTime WHERE voucherNo = '01'";
            string updateTimestampQuery = "UPDATE SyncTime SET newTimeStamp = @dt WHERE voucherNo = '01'";
            string insertDefaultRecordQuery = "INSERT INTO SyncTime (voucherNo, newTimeStamp) VALUES ('01', '2000-01-01')";

            try
            {
                using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    mssqlConnection.Open();

                    // Check if record exists
                    using (SqlCommand checkCommand = new SqlCommand(checkRecordExistsQuery, mssqlConnection))
                    {
                        int recordCount = (int)checkCommand.ExecuteScalar();

                        if (recordCount > 0)
                        {
                            // Record exists, update it
                            using (SqlCommand updateCommand = new SqlCommand(updateTimestampQuery, mssqlConnection))
                            {
                                updateCommand.Parameters.AddWithValue("@dt", (object)dt ?? DBNull.Value);
                                updateCommand.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // No record exists, insert default record
                            using (SqlCommand insertCommand = new SqlCommand(insertDefaultRecordQuery, mssqlConnection))
                            {
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<UpdateTimestamp> Error : {ex.Message}");
            }
        }

        //Insert Invoice Header
        public bool insertEinvoiceHeader(E_Invoice_Header header)
        {
            bool status = isInvoiceDoneUpdated(header.H_E_InvoiceNumber);

            if (!status)
            {
                string mssqlInsertQuery =
                "INSERT INTO E_Invoice_Header" +
                "(" +
                "SupplierName, SupplierTIN, SupplierID, SupplierSST," +
                "SupplierTourismTax, SupplierEmail, SupplierMSIC, SupplierBusinessActivityDesc," +
                "SupplierAddress0, SupplierAddress1, SupplierAddress2, SupplierPostal," +
                "SupplierCity, SupplierState, SupplierCountry, SupplierContactNo," +
                "BuyerName, BuyerTIN, BuyerID, BuyerSST, BuyerEmail," +
                "BuyerAddress0, BuyerAddress1, BuyerAddress2, BuyerPostal, BuyerCity, BuyerState," +
                "BuyerCountry, BuyerContactNo, H_E_InvoiceVersion, H_E_InvoiceType, H_E_InvoiceNumber," +
                "H_E_InvoiceDate, H_E_InvoiceTime, H_SupplierDigitalSignature, H_InvoiceCurCode," +
                "H_ExchangeRate, H_FrequencyBilling, H_BillingPeriodStartDate, H_BillingPeriodEndDate," +
                "H_PaymentMode, H_SupplierBankAC, H_PaymentTerms, H_PrePaymentAmount, H_PrePaymentDate," +
                "H_PrePaymentTime, H_PrePaymentRefNo, H_BillRefNo, H_TotalExcludingTax, H_TotalIncludingTax," +
                "H_TotalPayableAmount, H_TotalNetAmount, H_TotalDiscountValue, H_TotalChargeAmount, H_TotalTaxAmount," +
                "H_RoundingAmount, H_TotalTaxableAmtPerTaxType, H_TotalTaxAmtPerTaxType, H_TaxType, H_AdditionalDiscountAmount," +
                "H_AdditionalFeeAmount, H_ShippingName, H_ShippingAddress0, H_ShippingAddress1, H_ShippingAddress2," +
                "H_ShippingPostal, H_ShippingCity, H_ShippingState, H_ShippingCountry, H_ShippingTIN, " +
                "H_ShippingID, H_CustomForm1to9RefNo, H_FTAInfo, H_CertifiedExporterAuthNo, " +
                "H_CustomForm2, H_OtherChargesDetail, MySoft_ARAPCode, MySoft_IsSelfBill, MySoft_UserName," +
                "MySoft_LogDate," +
                "H_TaxExemption, H_TaxExemptionAmount, eInv_isSubmit, H_OriginalDocUUID, H_OriginalDocNo" +
                ") " +
                "VALUES " +
                "(" +
                "@SupplierName, @SupplierTIN, @SupplierID, @SupplierSST," +
                "@SupplierTourismTax, @SupplierEmail, @SupplierMSIC, @SupplierBusinessActivityDesc," +
                "@SupplierAddress0, @SupplierAddress1, @SupplierAddress2, @SupplierPostal," +
                "@SupplierCity, @SupplierState, @SupplierCountry, @SupplierContactNo," +
                "@BuyerName, @BuyerTIN, @BuyerID, @BuyerSST, @BuyerEmail," +
                "@BuyerAddress0, @BuyerAddress1, @BuyerAddress2, @BuyerPostal, @BuyerCity, @BuyerState," +
                "@BuyerCountry, @BuyerContactNo, @H_E_InvoiceVersion, @H_E_InvoiceType, @H_E_InvoiceNumber," +
                "@H_E_InvoiceDate, @H_E_InvoiceTime, @H_SupplierDigitalSignature, @H_InvoiceCurCode," +
                "@H_ExchangeRate, @H_FrequencyBilling, @H_BillingPeriodStartDate, @H_BillingPeriodEndDate," +
                "@H_PaymentMode, @H_SupplierBankAC, @H_PaymentTerms, @H_PrePaymentAmount, @H_PrePaymentDate," +
                "@H_PrePaymentTime, @H_PrePaymentRefNo, @H_BillRefNo, @H_TotalExcludingTax, @H_TotalIncludingTax," +
                "@H_TotalPayableAmount, @H_TotalNetAmount, @H_TotalDiscountValue, @H_TotalChargeAmount, @H_TotalTaxAmount," +
                "@H_RoundingAmount, @H_TotalTaxableAmtPerTaxType, @H_TotalTaxAmtPerTaxType, @H_TaxType, @H_AdditionalDiscountAmount," +
                "@H_AdditionalFeeAmount, @H_ShippingName, @H_ShippingAddress0, @H_ShippingAddress1, @H_ShippingAddress2," +
                "@H_ShippingPostal, @H_ShippingCity, @H_ShippingState, @H_ShippingCountry, @H_ShippingTIN, " +
                "@H_ShippingID, @H_CustomForm1to9RefNo, @H_FTAInfo, @H_CertifiedExporterAuthNo, " +
                "@H_CustomForm2, @H_OtherChargesDetail, @MySoft_ARAPCode, @MySoft_IsSelfBill, @MySoft_UserName," +
                "@MySoft_LogDate," +
                "@H_TaxExemption, @H_TaxExemptionAmount, @eInv_IsSubmit, @H_OriginalDocUUID, @H_OriginalDocNo" +
                ")"
                ;

                try
                {
                    using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                    {
                        using (SqlCommand mssqlInsertCommand = new SqlCommand(mssqlInsertQuery, mssqlConnection))
                        {
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierName", header.SupplierName ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierTIN", header.SupplierTIN ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierID", header.SupplierID ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierSST", header.SupplierSST ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierTourismTax", header.SupplierTourismTax ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierEmail", header.SupplierEmail ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierMSIC", header.SupplierMSIC ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierBusinessActivityDesc", header.SupplierBusinessActivityDesc ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierAddress0", header.SupplierAddress0 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierAddress1", header.SupplierAddress1 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierAddress2", header.SupplierAddress2 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierPostal", header.SupplierPostal ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierCity", header.SupplierCity ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierState", header.SupplierState ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierCountry", header.SupplierCountry ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierContactNo", header.SupplierContactNo ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerName", header.BuyerName ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerTIN", header.BuyerTIN ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerID", header.BuyerID ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerSST", header.BuyerSST ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerEmail", header.BuyerEmail ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerAddress0", header.BuyerAddress0 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerAddress1", header.BuyerAddress1 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerAddress2", header.BuyerAddress2 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerPostal", header.BuyerPostal ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerCity", header.BuyerCity ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerState", header.BuyerState ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerCountry", header.BuyerCountry ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerContactNo", header.BuyerContactNo ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_E_invoiceVersion", header.H_E_InvoiceVersion ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_E_invoiceType", header.H_E_InvoiceType ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_E_invoiceNumber", header.H_E_InvoiceNumber ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_E_invoiceDate", header.H_E_InvoiceDate ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_E_invoiceTime", header.H_E_InvoiceTime ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_SupplierDigitalSignature", header.H_SupplierDigitalSignature ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_InvoiceCurCode", header.H_InvoiceCurCode ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ExchangeRate", header.H_ExchangeRate);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_FrequencyBilling", header.H_FrequencyBilling ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_BillingPeriodStartDate", DBNull.Value);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_BillingPeriodEndDate", DBNull.Value);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_PaymentMode", header.H_PaymentMode ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_SupplierBankAC", header.H_SupplierBankAC ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_PaymentTerms", header.H_PaymentTerms ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_PrePaymentAmount", header.H_PrePaymentAmount ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_PrePaymentDate", DBNull.Value);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_PrePaymentTime", DBNull.Value);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_PrePaymentRefNo", header.H_PrePaymentRefNo ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_BillRefNo", header.H_BillRefNo ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalExcludingTax", header.H_TotalExcludingTax);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalIncludingTax", header.H_TotalIncludingTax);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalPayableAmount", header.H_TotalPayableAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalNetAmount", header.H_TotalNetAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalDiscountValue", header.H_TotalDiscountValue);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalChargeAmount", header.H_TotalChargeAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalTaxAmount", header.H_TotalTaxAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_RoundingAmount", header.H_RoundingAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalTaxableAmtPerTaxType", header.H_TotalTaxableAmtPerTaxType);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TotalTaxAmtPerTaxType", header.H_TotalTaxAmtPerTaxType);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TaxType", header.H_TaxType ?? "02");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_AdditionalDiscountAmount", header.H_AdditionalDiscountAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_AdditionalFeeAmount", header.H_AdditionalFeeAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingName", header.H_ShippingName ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingAddress0", header.H_ShippingAddress0 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingAddress1", header.H_ShippingAddress1 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingAddress2", header.H_ShippingAddress2 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingPostal", header.H_ShippingPostal ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingCity", header.H_ShippingCity ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingState", header.H_ShippingState ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingCountry", header.H_ShippingCountry ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingTIN", header.H_ShippingTIN ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_ShippingID", header.H_ShippingID ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_CustomForm1to9RefNo", header.H_CustomForm1to9RefNo ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_FTAInfo", header.H_FTAInfo ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_CertifiedExporterAuthNo", header.H_CertifiedExporterAuthNo ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_CustomForm2", header.H_CustomForm2 ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_OtherChargesDetail", header.H_OtherChargesDetail ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@MySoft_ARAPCode", header.MySoft_ARAPCode ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@MySoft_IsSelfBill", header.MySoft_IsSelfBill);
                            mssqlInsertCommand.Parameters.AddWithValue("@MySoft_UserName", header.MySoft_UserName ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@MySoft_LogDate", header.MySoft_LogDate == default(DateTime) ? DateTime.Now : header.MySoft_LogDate);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TaxExemption", header.H_TaxExemption ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_TaxExemptionAmount", header.H_TaxExemptionAmount ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@eInv_isSubmit", header.eInv_isSubmit);
                            mssqlInsertCommand.Parameters.AddWithValue("@H_OriginalDocUUID", header.H_OriginalDocUUID ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@H_OriginalDocNo", header.H_OriginalDocNo ?? "");

                            mssqlConnection.Open();
                            int rowsAffected = mssqlInsertCommand.ExecuteNonQuery();

                            LogMessage($"<insertEinvoiceHeader> {rowsAffected} row(s) inserted.");

                            mssqlConnection.Close();
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"<insertEinvoiceHeader> Error : {ex.Message} ");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // Update Invoice Header
        public bool updateEinvoiceHeader(E_Invoice_Header header)
        {
            bool status = isInvoiceDoneUpdated(header.H_E_InvoiceNumber.ToString());
            if (status == false)
            {
                string mssqlUpdateQuery =
                    "UPDATE E_Invoice_Header SET " +
                    "SupplierName = @SupplierName, SupplierTIN = @SupplierTIN, SupplierID = @SupplierID, SupplierSST = @SupplierSST," +
                    "SupplierTourismTax = @SupplierTourismTax, SupplierEmail = @SupplierEmail, SupplierMSIC = @SupplierMSIC, SupplierBusinessActivityDesc = @SupplierBusinessActivityDesc," +
                    "SupplierAddress0 = @SupplierAddress0, SupplierAddress1 = @SupplierAddress1, SupplierAddress2 = @SupplierAddress2, SupplierPostal = @SupplierPostal," +
                    "SupplierCity = @SupplierCity, SupplierState = @SupplierState, SupplierCountry = @SupplierCountry, SupplierContactNo = @SupplierContactNo," +
                    "BuyerName = @BuyerName, BuyerTIN = @BuyerTIN, BuyerID = @BuyerID, BuyerSST = @BuyerSST, BuyerEmail = @BuyerEmail, " +
                    "BuyerAddress0 = @BuyerAddress0, BuyerAddress1 = @BuyerAddress1, BuyerAddress2 = @BuyerAddress2, BuyerPostal = @BuyerPostal,BuyerCity = @BuyerCity, BuyerState = @BuyerState, " +
                    "BuyerCountry = @BuyerCountry, BuyerContactNo = @BuyerContactNo, H_E_InvoiceVersion = @H_E_InvoiceVersion, H_E_InvoiceType = @H_E_InvoiceType, H_E_InvoiceDate = @H_E_InvoiceDate, " +
                    "H_E_InvoiceTime = @H_E_InvoiceTime, H_SupplierDigitalSignature = @H_SupplierDigitalSignature, H_InvoiceCurCode = @H_InvoiceCurCode, H_ExchangeRate = @H_ExchangeRate, H_FrequencyBilling = @H_FrequencyBilling, " +
                    "H_BillingPeriodStartDate = @H_BillingPeriodStartDate, H_BillingPeriodEndDate = @H_BillingPeriodEndDate, H_PaymentMode = @H_PaymentMode, H_SupplierBankAC = @H_SupplierBankAC, H_PaymentTerms = @H_PaymentTerms, " +
                    "H_PrePaymentAmount = @H_PrePaymentAmount, H_PrePaymentDate = @H_PrePaymentDate, H_PrePaymentTime = @H_PrePaymentTime, H_PrePaymentRefNo = @H_PrePaymentRefNo, H_BillRefNo = @H_BillRefNo, " +
                    "H_TotalExcludingTax = @H_TotalExcludingTax, H_TotalIncludingTax = @H_TotalIncludingTax, H_TotalPayableAmount = @H_TotalPayableAmount, H_TotalNetAmount = @H_TotalNetAmount, H_TotalDiscountValue = @H_TotalDiscountValue, " +
                    "H_TotalChargeAmount = @H_TotalChargeAmount, H_TotalTaxAmount = @H_TotalTaxAmount, H_RoundingAmount = @H_RoundingAmount, H_TotalTaxableAmtPerTaxType = @H_TotalTaxableAmtPerTaxType, H_TotalTaxAmtPerTaxType = @H_TotalTaxAmtPerTaxType, " +
                    "H_TaxType = @H_TaxType, H_AdditionalDiscountAmount = @H_AdditionalDiscountAmount, H_AdditionalFeeAmount = @H_AdditionalFeeAmount, H_ShippingName = @H_ShippingName, H_ShippingAddress0 = @H_ShippingAddress0, " +
                    "H_ShippingAddress1 = @H_ShippingAddress1, H_ShippingAddress2 = @H_ShippingAddress2, H_ShippingPostal = @H_ShippingPostal, H_ShippingCity = @H_ShippingCity, H_ShippingState = @H_ShippingState, " +
                    "H_ShippingCountry = @H_ShippingCountry, H_ShippingTIN = @H_ShippingTIN, H_ShippingID = @H_ShippingID, H_CustomForm1to9RefNo = @H_CustomForm1to9RefNo, " +
                    "H_FTAInfo = @H_FTAInfo, H_CertifiedExporterAuthNo = @H_CertifiedExporterAuthNo, H_CustomForm2 = @H_CustomForm2, H_OtherChargesDetail = @H_OtherChargesDetail, MySoft_ARAPCode = @MySoft_ARAPCode, " +
                    "MySoft_IsSelfBill = @MySoft_IsSelfBill, MySoft_UserName = @MySoft_UserName, MySoft_LogDate = @MySoft_LogDate, H_OriginalDocUUID = @H_OriginalDocUUID, " +
                    "H_OriginalDocNo = @H_OriginalDocNo ,H_TaxExemption = @H_TaxExemption, " +
                    "H_TaxExemptionAmount = @H_TaxExemptionAmount " +
                    "WHERE H_BillRefNo = @H_E_InvoiceNumber"
                    ;


                try
                {
                    using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                    {
                        using (SqlCommand mssqlUpdateCommand = new SqlCommand(mssqlUpdateQuery, mssqlConnection))
                        {
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierName", header.SupplierName ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierTIN", header.SupplierTIN ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierID", header.SupplierID ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierSST", header.SupplierSST ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierTourismTax", header.SupplierTourismTax ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierEmail", header.SupplierEmail ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierMSIC", header.SupplierMSIC ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierBusinessActivityDesc", header.SupplierBusinessActivityDesc ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierAddress0", header.SupplierAddress0 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierAddress1", header.SupplierAddress1 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierAddress2", header.SupplierAddress2 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierPostal", header.SupplierPostal ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierCity", header.SupplierCity ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierState", header.SupplierState ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierCountry", header.SupplierCountry ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@SupplierContactNo", header.SupplierContactNo ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerName", header.BuyerName ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerTIN", header.BuyerTIN ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerID", header.BuyerID ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerSST", header.BuyerSST ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerEmail", header.BuyerEmail ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerAddress0", header.BuyerAddress0 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerAddress1", header.BuyerAddress1 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerAddress2", header.BuyerAddress2 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerPostal", header.BuyerPostal ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerCity", header.BuyerCity ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerState", header.BuyerState ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerCountry", header.BuyerCountry ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@BuyerContactNo", header.BuyerContactNo ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_E_invoiceVersion", header.H_E_InvoiceVersion ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_E_invoiceType", header.H_E_InvoiceType ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_E_invoiceNumber", header.H_E_InvoiceNumber ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_E_invoiceDate", header.H_E_InvoiceDate ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_E_invoiceTime", header.H_E_InvoiceTime ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_SupplierDigitalSignature", header.H_SupplierDigitalSignature ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_InvoiceCurCode", header.H_InvoiceCurCode ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ExchangeRate", header.H_ExchangeRate);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_FrequencyBilling", header.H_FrequencyBilling ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_BillingPeriodStartDate", DBNull.Value);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_BillingPeriodEndDate", DBNull.Value);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_PaymentMode", header.H_PaymentMode ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_SupplierBankAC", header.H_SupplierBankAC ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_PaymentTerms", header.H_PaymentTerms ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_PrePaymentAmount", header.H_PrePaymentAmount ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_PrePaymentDate", DBNull.Value);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_PrePaymentTime", DBNull.Value);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_PrePaymentRefNo", header.H_PrePaymentRefNo ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_BillRefNo", header.H_BillRefNo ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalExcludingTax", header.H_TotalExcludingTax);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalIncludingTax", header.H_TotalIncludingTax);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalPayableAmount", header.H_TotalPayableAmount);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalNetAmount", header.H_TotalNetAmount);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalDiscountValue", header.H_TotalDiscountValue);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalChargeAmount", header.H_TotalChargeAmount);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalTaxAmount", header.H_TotalTaxAmount);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_RoundingAmount", header.H_RoundingAmount);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalTaxableAmtPerTaxType", header.H_TotalTaxableAmtPerTaxType);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TotalTaxAmtPerTaxType", header.H_TotalTaxAmtPerTaxType);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TaxType", header.H_TaxType);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_AdditionalDiscountAmount", header.H_AdditionalDiscountAmount);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_AdditionalFeeAmount", header.H_AdditionalFeeAmount);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingName", header.H_ShippingName ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingAddress0", header.H_ShippingAddress0 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingAddress1", header.H_ShippingAddress1 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingAddress2", header.H_ShippingAddress2 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingPostal", header.H_ShippingPostal ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingCity", header.H_ShippingCity ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingState", header.H_ShippingState ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingCountry", header.H_ShippingCountry ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingTIN", header.H_ShippingTIN ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_ShippingID", header.H_ShippingID ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_CustomForm1to9RefNo", header.H_CustomForm1to9RefNo ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_FTAInfo", header.H_FTAInfo ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_CertifiedExporterAuthNo", header.H_CertifiedExporterAuthNo ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_CustomForm2", header.H_CustomForm2 ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_OtherChargesDetail", header.H_OtherChargesDetail ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@MySoft_ARAPCode", header.MySoft_ARAPCode ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@MySoft_IsSelfBill", header.MySoft_IsSelfBill);
                            mssqlUpdateCommand.Parameters.AddWithValue("@MySoft_UserName", header.MySoft_UserName ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@MySoft_LogDate", header.MySoft_LogDate == default(DateTime) ? DateTime.Now : header.MySoft_LogDate);
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TaxExemption", header.H_TaxExemption ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_TaxExemptionAmount", header.H_TaxExemptionAmount ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_OriginalDocUUID", header.H_OriginalDocUUID ?? "");
                            mssqlUpdateCommand.Parameters.AddWithValue("@H_OriginalDocNo", header.H_OriginalDocNo ?? "");

                            mssqlConnection.Open();
                            int rowsAffected = mssqlUpdateCommand.ExecuteNonQuery();

                            LogMessage($"<updateEinvoiceHeader> {rowsAffected} row(s) updated.");

                            mssqlConnection.Close();
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"<updateEinvoiceHeader> Error : {ex.Message} ");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // Delete Invoice Header and the Detail
        public void DeleteInvoiceHeaderAndDetail(string Voucher)
        {
            bool status = isInvoiceDoneUpdated(Voucher);

            if (!status)
            {
                string deleteHeaderQuery = "DELETE FROM E_Invoice_Header WHERE H_BillRefNo = @Voucher";
                string deleteDetailQuery = "DELETE FROM E_Invoice_Detail WHERE H_BillRefNo = @Voucher";

                try
                {
                    using (var connection = new SqlConnection(mssqlConnectionString))
                    {
                        connection.Open();

                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Delete from E_Invoice_Header
                                using (var command = new SqlCommand(deleteHeaderQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Voucher", Voucher);
                                    command.ExecuteNonQuery();
                                }

                                // Delete from E_Invoice_Detail
                                using (var command = new SqlCommand(deleteDetailQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Voucher", Voucher);
                                    command.ExecuteNonQuery();
                                }

                                // Commit the transaction
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                // Rollback the transaction in case of an error
                                transaction.Rollback();
                                LogMessage($"<DeleteInvoiceHeaderDetail> Error: {ex.Message}");
                                throw; // Rethrow the exception after logging it
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"<DeleteInvoiceHeaderDetail> Error: {ex.Message}");
                }
            }
        }

        // Delete Invoice Detail
        public void DeleteInvoiceDetail(string Voucher)
        {
            bool status = isInvoiceDoneUpdated(Voucher);

            if (!status)
            {
                //string deleteDetailQuery = "DELETE FROM E_Invoice_Detail WHERE H_E_InvoiceNumber = @Voucher";
                string deleteDetailQuery = "DELETE t1 FROM E_Invoice_Detail t1 LEFT JOIN E_Invoice_Header t2 on  t2.H_E_InvoiceNumber = t1.H_E_InvoiceNumber WHERE t2.H_BillRefNo = @Voucher";
                try
                {
                    using (var connection = new SqlConnection(mssqlConnectionString))
                    {
                        connection.Open();

                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Delete from E_Invoice_Detail
                                using (var command = new SqlCommand(deleteDetailQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Voucher", Voucher);
                                    command.ExecuteNonQuery();
                                }

                                // Commit the transaction
                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                // Rollback the transaction in case of an error
                                transaction.Rollback();
                                LogMessage($"<DeleteInvoiceDetail> Error: {ex.Message}");
                                throw; // Rethrow the exception after logging it
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"<DeleteInvoiceDetail> Error: {ex.Message}");
                }
            }
        }

        // Insert Invoice Detail
        public bool insertEinvoiceItem(E_Invoice_Detail detail)
        {
            bool status = isInvoiceDoneUpdated(detail.H_E_InvoiceNumber);

            if (!status)
            {
                string mssqlInsertQuery =
                    "INSERT INTO E_Invoice_Detail" +
                    "(" +
                    "H_E_InvoiceNumber, SupplierTIN, BuyerTIN, LI_ProductClassification, LI_ProductDescription," +
                    "LI_UnitPrice, LI_TaxType, LI_TaxRate, LI_TaxAmount, LI_TaxExemption," +
                    "LI_TaxExemptionAmount, LI_SubTotal, LI_TotalExcludingTax, LI_Quantity, LI_UOM," +
                    "LI_DiscountRate, LI_DiscountAmount, LI_ChargeRate, LI_ChargeAmount, LI_ProductTariffCode," +
                    "LI_CountryOrigin, MySoft_StockCode, MySoft_LineNo" +
                    ")" +
                    "VALUES " +
                    "(" +
                    "@H_E_InvoiceNumber, @SupplierTIN, @BuyerTIN, @LI_ProductClassification, @LI_ProductDescription," +
                    "@LI_UnitPrice, @LI_TaxType, @LI_TaxRate, @LI_TaxAmount, @LI_TaxExemption," +
                    "@LI_TaxExemptionAmount, @LI_SubTotal, @LI_TotalExcludingTax, @LI_Quantity, @LI_UOM," +
                    "@LI_DiscountRate, @LI_DiscountAmount, @LI_ChargeRate, @LI_ChargeAmount, @LI_ProductTariffCode," +
                    "@LI_CountryOrigin, @MySoft_StockCode, @MySoft_LineNo" +
                    ")"
                    ;

                try
                {
                    using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                    {
                        using (SqlCommand mssqlInsertCommand = new SqlCommand(mssqlInsertQuery, mssqlConnection))
                        {
                            mssqlInsertCommand.Parameters.AddWithValue("@H_E_InvoiceNumber", detail.H_E_InvoiceNumber ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@SupplierTIN", detail.SupplierTIN ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@BuyerTIN", detail.BuyerTIN ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_ProductClassification", detail.LI_ProductClassification ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_ProductDescription", detail.LI_ProductDescription ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_UnitPrice", detail.LI_UnitPrice);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_TaxType", detail.LI_TaxType);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_TaxRate", detail.LI_TaxRate);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_TaxAmount", detail.LI_TaxAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_TaxExemption", detail.LI_TaxExemption ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_TaxExemptionAmount", detail.LI_TaxExemptionAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_SubTotal", detail.LI_SubTotal);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_TotalExcludingTax", detail.LI_TotalExcludingTax);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_Quantity", detail.LI_Quantity);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_UOM", detail.LI_UOM ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_DiscountRate", detail.LI_DiscountRate);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_DiscountAmount", detail.LI_DiscountAmount);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_ChargeRate", detail.LI_ChargeRate);
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_ChargeAmount", Math.Round(detail.LI_ChargeAmount, 2));
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_ProductTariffCode", detail.LI_ProductTariffCode ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@LI_CountryOrigin", detail.LI_CountryOrigin ?? "");
                            mssqlInsertCommand.Parameters.AddWithValue("@MySoft_StockCode", detail.MySoft_StockCode);
                            mssqlInsertCommand.Parameters.AddWithValue("@MySoft_LineNo", detail.MySoft_LineNo);

                            mssqlConnection.Open();
                            int rowsAffected = mssqlInsertCommand.ExecuteNonQuery();

                            LogMessage($"<insertEinvoiceItem> {rowsAffected} row(s) inserted.");

                            mssqlConnection.Close();
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LogMessage($"<insertEinvoiceItem> Error : {ex.Message} ");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // Check the customer is it exist in the mssql
        public bool CheckCustomerCodeExists(string customerCode)
        {
            string mssqlCheckQuery = "SELECT COUNT(*) FROM Customer WHERE ar_code = @CustomerCode";

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand mssqlCheckCommand = new SqlCommand(mssqlCheckQuery, mssqlConnection))
                    {
                        // Set the parameter value to prevent SQL injection
                        mssqlCheckCommand.Parameters.AddWithValue("@CustomerCode", customerCode ?? string.Empty);

                        mssqlConnection.Open();

                        // Execute the query and get the result
                        var result = (int)mssqlCheckCommand.ExecuteScalar(); // ExecuteScalar to get the count

                        // Return true if count is greater than 0, indicating the customer exists
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<CheckCustomerCodeExists> Error: {ex.Message}");
            }

            // Return false if an exception occurs
            return false;
        }

        // Insert Customer
        public void insertCustomer(Customer customer)
        {
            string mssqlInsertQuery =
                "INSERT INTO Customer" +
                "(" +
                "ar_code, customer_name, Salesman_code, statement_type, Credit_Limit," +
                "credit_period, dcc, sales_code, Tax_type, DeptCode," +
                "JobCode, UptoDateSales, UptoDatePaid, Prepay, Balance," +
                "LastEntryDate, RptPrintOption, Points, CurCode, Trans," +
                "UpFlag, AccountStatus, IsInActive, Roundingadj, AutoSelectEmail," +
                "DueMonth, B2b_INVPOWS_Active, SellPriceMarkUpByItemType, Classification, PersonInCharge," +
                "TIN, E_IDType, E_IDNumber, SSTNo, E_mail, Address1, Address2, Address3, PostCode," +
                "City, E_StateCode, E_CountryCode, telephone_no" +
                ") " +
                "VALUES " +
                "(" +
                "@CustomerCode, @CustomerName, 0, 'OPEN ITEM', 0," +
                "'30 Days', 12100, 51100, 'CPE', 0," +
                "0, 0, 0, 0, 0," +
                "@EntryDate, 0, 0, 'MYR', 0," +
                "0, 1, 0, 0, 0," +
                "0, 0, 0, 0, 0," +
                "@CustomerTIN, @CustomerIDType, @CustomerID, @CustomerSST, @CustomerEmail, @CustomerAdd1, @CustomerAdd2, @CustomerAdd3, @CustomerPostal," +
                "@CustomerCity, @CustomerState, @CustomerCountry, @CustomerContact" +
                ")"
            ;

            try
            {
                Console.WriteLine(customer.BuyerCode);
                using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand mssqlInsertCommand = new SqlCommand(mssqlInsertQuery, mssqlConnection))
                    {
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerCode", customer.BuyerCode ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerName", customer.BuyerName ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerTIN", customer.BuyerTin ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerIDType", customer.BuyerIDType ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerID", customer.BuyerID ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerSST", customer.BuyerSST ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerEmail", customer.BuyerEmail ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerAdd1", customer.BuyerAddress0 ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerAdd2", customer.BuyerAddress1 ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerAdd3", customer.BuyerAddress2 ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerPostal", customer.BuyerPostal ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerCity", customer.BuyerCity ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerState", customer.BuyerState ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerCountry", customer.BuyerCountry ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@CustomerContact", customer.BuyerContactNo ?? "");
                        string formattedDate = DateTime.Now.ToString("ddMMyyyy");
                        mssqlInsertCommand.Parameters.AddWithValue("@EntryDate", DateTime.Now);


                        mssqlConnection.Open();
                        int rowsAffected = mssqlInsertCommand.ExecuteNonQuery();

                        LogMessage($"<insertCustomer> {rowsAffected} row(s) inserted.");

                        mssqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<insertCustomer> Error : {ex.Message} ");
            }
        }

        // Update Customer
        public void updateCustomer(Customer customer)
        {
            string mssqlUpdateQuery =
            "UPDATE Customer " +
            "SET " +
            "customer_name = @CustomerName, " +
            "TIN = @CustomerTIN," +
            "E_IDType = @CustomerIDType," +
            "E_IDNumber = @CustomerIDNumber," +
            "SSTNo = @CustomerSSTNo," +
            "E_mail = @CustomerEmail," +
            "Address1 = @CustomerAdd1," +
            "Address2 = @CustomerAdd2," +
            "Address3 = @CustomerAdd3," +
            "PostCode = @CustomerPostal," +
            "City = @CustomerCity," +
            "E_StateCode = @CustomerState," +
            "E_CountryCode = @CustomerCountry," +
            "telephone_no = @CustomerTel " +
            "WHERE ar_code = @CustomerCode";

            try
            {
                using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                using (SqlCommand mssqlUpdateCommand = new SqlCommand(mssqlUpdateQuery, mssqlConnection))
                {
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerCode", customer.BuyerCode ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerName", customer.BuyerName ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerTIN", customer.BuyerTin ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerIDType", customer.BuyerIDType ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerIDNumber", customer.BuyerID ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerSSTNo", customer.BuyerSST ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerEmail", customer.BuyerEmail ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerAdd1", customer.BuyerAddress0 ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerAdd2", customer.BuyerAddress1 ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerAdd3", customer.BuyerAddress2 ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerPostal", customer.BuyerPostal ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerCity", customer.BuyerCity ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerState", customer.BuyerState ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerCountry", customer.BuyerCountry ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@CustomerTel", customer.BuyerContactNo ?? "");

                    mssqlConnection.Open();
                    int rowsAffected = mssqlUpdateCommand.ExecuteNonQuery();

                    LogMessage($"<updateCustomer> {rowsAffected} row(s) updated.");

                    mssqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[{DateTime.Now}] <updateCustomer> Error : {ex.Message} ");
            }
        }

        // Check the product is it exist in the mssql
        public bool CheckProductCodeExists(string productCode)
        {
            string mssqlCheckQuery = "SELECT COUNT(*) FROM StockMasterDetails WHERE Stock_Code = @StockCode";

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand mssqlCheckCommand = new SqlCommand(mssqlCheckQuery, mssqlConnection))
                    {
                        // Set the parameter value to prevent SQL injection
                        mssqlCheckCommand.Parameters.AddWithValue("@StockCode", productCode ?? string.Empty);

                        mssqlConnection.Open();

                        // Execute the query and get the result
                        var result = (int)mssqlCheckCommand.ExecuteScalar(); // ExecuteScalar to get the count

                        // Return true if count is greater than 0, indicating the product exists
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<CheckProductCodeExists> Error: {ex.Message}");
            }

            // Return false if an exception occurs
            return false;
        }

        // Insert Product
        public void insertProduct(Product product)
        {
            string mssqlInsertQuery =
                "INSERT INTO StockMasterDetails" +
                "(" +
                "Stock_Code, Stock_Cat, Description, Tax_Code, Unit_Weight," +
                "Dept_Number, Nominal_Code, Qty_Allocated, Qty_In_Stock, Qty_Last_Order," +
                "Qty_Last_Stock_Take, Qty_On_Order, Qty_Reorder_Level, Qty_Max_Lever, Qty_Min_Lever," +
                "Web_Publish, Web_Special_Offer, Item_Type,Last_Purchase_Price, Last_Tran," +
                "WarehouseCode, Selling_Price, Cost_Price, ForeignSellPrice, ForgnCostCurr," +
                "Sman_Comm, Job_Code, BatchQty, PurchaseCode, IsActive," +
                "ExchangeRate, BaseRate, ForeignRate, IsWarranty, Warrenty," +
                "NetWeight, Volume, LifeTime, GroupCode, ClassCode," +
                "PackFactor, OtherCost, Avg_CostPrice, LastStockInQty, IsSalesTaxAcc," +
                "ImportDutyPer, SalesTaxPer, UpFlag, CJQuantity, MinSellingPrice," +
                "POSPrice, Price1, Price2, Price3, Price4," +
                "PurchaseTax_Code, InclusiveTax, DiscountGLCode, isPrescribedGoods, RollingAvgCost," +
                "OnlineSellingPrice, isEcommerceProduct, SalesVolume, ExpiryDate, NoMonthConsume," +
                "HoldingNoMonth, isSlowMovingStock, SubProductQty, SubProductCN" +
                ") " +
                "VALUES " +
                "(" +
                "@ProductCode, 0, @ProductDescription, 'CPE', 0," +
                "0, 51100, 0, 0, 0," +
                "0, 0, 0, 0, 0," +
                "0, 0, 'NonStock', 0, 0," +
                "'none', @ProductPrice, 0.1, 0, 'MYR'," +
                "0, 0, 0, 61100, 0," +
                "0, 0, 0, 0, 0," +
                "0, 0, 0, 0, 0," +
                "0, 0, 0, 0, 0," +
                "0, 0, 0, 0, 0," +
                "0, 0, 0, 0, 0," +
                "'CPE', 0, '52200', 0, 0," +
                "0, 0, 0, @Expired, 0," +
                "0, 0, 0, 0" +
                ")"
            ;

            try
            {
                using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand mssqlInsertCommand = new SqlCommand(mssqlInsertQuery, mssqlConnection))
                    {
                        mssqlInsertCommand.Parameters.AddWithValue("@ProductCode", product.ProductCode ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@ProductDescription", product.ProductDescription ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@ProductPrice", product.ProductPrice ?? "");
                        mssqlInsertCommand.Parameters.AddWithValue("@Expired", 31 / 12 / 2999);


                        mssqlConnection.Open();
                        int rowsAffected = mssqlInsertCommand.ExecuteNonQuery();

                        LogMessage($"<insertProduct> {rowsAffected} row(s) inserted.");

                        mssqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<insertProduct> Error : {ex.Message} ");
            }
        }

        // Update Product
        public void updateProduct(Product product)
        {
            string mssqlUpdateQuery =
            "UPDATE StockMasterDetails " +
            "SET " +
            "Description = @ProductDescription, " +
            "Selling_Price = @ProductPrice " +
            "WHERE Stock_Code = @ProductCode";

            try
            {
                using (var mssqlConnection = new SqlConnection(mssqlConnectionString))
                using (SqlCommand mssqlUpdateCommand = new SqlCommand(mssqlUpdateQuery, mssqlConnection))
                {
                    mssqlUpdateCommand.Parameters.AddWithValue("@ProductCode", product.ProductCode ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@ProductDescription", product.ProductDescription ?? "");
                    mssqlUpdateCommand.Parameters.AddWithValue("@ProductPrice", product.ProductPrice ?? "");


                    mssqlConnection.Open();
                    int rowsAffected = mssqlUpdateCommand.ExecuteNonQuery();

                    LogMessage($"<updateProduct> {rowsAffected} row(s) updated.");

                    mssqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[{DateTime.Now}] <updateProduct> Error : {ex.Message} ");
            }
        }

        // Get the Company Detail from the Mssql
        public CompanyProfile getSupplierDetails()
        {
            CompanyProfile companyProfile = new CompanyProfile();
            string supplierQuery = "SELECT Name, TIN, CompanyNoNew, " +
                                   "SSTNo, EInvEmail, MSIC, BusinessActivityDesc," +
                                   "Address1, Address2, Address3, PostCode, City," +
                                   "E_StateCode, E_CountryCode, E_PhoneNo FROM CompanyProfile";



            using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
            {
                using (SqlCommand mssqlCheckSupplierCommand = new SqlCommand(supplierQuery, mssqlConnection))
                {
                    mssqlConnection.Open();

                    // Execute the query and get the result set
                    using (SqlDataReader reader = mssqlCheckSupplierCommand.ExecuteReader())
                    {
                        // Check if there are any rows returned
                        if (reader.HasRows)
                        {
                            // Read each row
                            while (reader.Read())
                            {
                                // Get the value of Invoice_Code column and add it to the list
                                companyProfile.SupplierName = string.IsNullOrWhiteSpace(reader["Name"].ToString()) ? "" : reader["Name"].ToString();
                                companyProfile.SupplierTIN = string.IsNullOrWhiteSpace(reader["TIN"].ToString()) ? "" : reader["TIN"].ToString();
                                companyProfile.SupplierID = string.IsNullOrWhiteSpace(reader["CompanyNoNew"].ToString()) ? "" : reader["CompanyNoNew"].ToString();
                                companyProfile.SupplierSST = string.IsNullOrWhiteSpace(reader["SSTNo"].ToString()) ? "" : reader["SSTNo"].ToString();
                                companyProfile.SupplierEmail = string.IsNullOrWhiteSpace(reader["EInvEmail"].ToString()) ? "" : reader["EInvEmail"].ToString();
                                companyProfile.SupplierMSIC = string.IsNullOrWhiteSpace(reader["MSIC"].ToString()) ? "00000" : reader["MSIC"].ToString();
                                companyProfile.SupplierBusinessActivityDesc = string.IsNullOrWhiteSpace(reader["BusinessActivityDesc"].ToString()) ? "" : reader["BusinessActivityDesc"].ToString();
                                companyProfile.SupplierAddress0 = string.IsNullOrWhiteSpace(reader["Address1"].ToString()) ? "" : reader["Address1"].ToString();
                                companyProfile.SupplierAddress1 = string.IsNullOrWhiteSpace(reader["Address2"].ToString()) ? "" : reader["Address2"].ToString();
                                companyProfile.SupplierAddress2 = string.IsNullOrWhiteSpace(reader["Address3"].ToString()) ? "" : reader["Address3"].ToString();
                                companyProfile.SupplierPostal = string.IsNullOrWhiteSpace(reader["PostCode"].ToString()) ? "" : reader["PostCode"].ToString();
                                companyProfile.SupplierCity = string.IsNullOrWhiteSpace(reader["City"].ToString()) ? "N/A" : reader["City"].ToString();
                                companyProfile.SupplierState = string.IsNullOrWhiteSpace(reader["E_StateCode"].ToString()) ? "00" : reader["E_StateCode"].ToString();
                                companyProfile.SupplierCountry = string.IsNullOrWhiteSpace(reader["E_CountryCode"].ToString()) ? "MYS" : reader["E_CountryCode"].ToString();
                                companyProfile.SupplierContactNo = string.IsNullOrWhiteSpace(reader["E_PhoneNo"].ToString()) ? "" : reader["E_PhoneNo"].ToString();
                            }
                        }
                    }
                    // Close the connection
                    mssqlConnection.Close();
                }
            }

            // Return the company info 
            return companyProfile;

        }

        // Get Product classification in the mssql if dont have use defaul "022"
        public async Task<string> GetProductClassificationAsync(string productCode)
        {
            string defaultClassification = "022";
            string productQuery = "SELECT Classification FROM StockMasterDetails WHERE Stock_Code=@StockCode";

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    await mssqlConnection.OpenAsync(); // Open the connection asynchronously

                    using (SqlCommand mssqlCheckProductCommand = new SqlCommand(productQuery, mssqlConnection))
                    {
                        // Parameterized query to prevent SQL injection
                        mssqlCheckProductCommand.Parameters.AddWithValue("@StockCode", productCode ?? string.Empty);

                        // Execute the query asynchronously
                        using (SqlDataReader reader = await mssqlCheckProductCommand.ExecuteReaderAsync())
                        {
                            // If a row is returned, check the Classification field
                            if (await reader.ReadAsync())
                            {
                                // Return classification if found, otherwise return default
                                return string.IsNullOrWhiteSpace(reader["Classification"].ToString())
                                    ? defaultClassification
                                    : reader["Classification"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<GetProductClassificationAsync> Error: {ex.Message}");
            }

            // If no rows or an error occurs, return the default classification
            return defaultClassification;
        }

        // Check the invoice in the mssql is already Valid or Submitted to LHDN
        public bool isInvoiceDoneUpdated(string invoiceNumber)
        {
            // Query to get the submission status
            string submissionStatusQuery = "SELECT SubmissionStatus FROM E_Invoice_Header WHERE H_E_InvoiceNumber = @InvoiceNumber";

            // Define the list of valid statuses
            string[] validStatus = { "VALID", "CONSOLIDATED", "SUBMITTED", "CANCELLED" };

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand mssqlCheckProductCommand = new SqlCommand(submissionStatusQuery, mssqlConnection))
                    {
                        // Set the parameter value
                        mssqlCheckProductCommand.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

                        // Open the connection
                        mssqlConnection.Open();

                        // Execute the query and get the result set
                        using (SqlDataReader reader = mssqlCheckProductCommand.ExecuteReader())
                        {
                            // Check if a row exists
                            if (reader.Read())
                            {
                                // Get the value of SubmissionStatus, handle null and empty values
                                string submissionStatus = reader["SubmissionStatus"] as string ?? "";

                                // Check if the submissionStatus is in the valid status list
                                if (Array.Exists(validStatus, status => status.Equals(submissionStatus, StringComparison.OrdinalIgnoreCase)))
                                {
                                    return true; // Valid status
                                }
                            }
                        }

                        // Close the connection
                        mssqlConnection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<isInvoiceUpdated> Error: {ex.Message}");
            }

            // If the status is null, empty, or not valid, return false
            return false;
        }

        // Check the invocie is it exist in the mssql
        public bool isInvoiceHeaderExist(string invoiceNumber)
        {
            // Query to check if the invoice exists by counting matching rows
            string query = "SELECT COUNT(*) FROM E_Invoice_Header WHERE H_BillRefNo = @InvoiceNumber";

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, mssqlConnection))
                    {
                        // Set the parameter value
                        command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

                        // Open the connection
                        mssqlConnection.Open();

                        // Use ExecuteScalar to get the count of matching rows
                        int count = (int)command.ExecuteScalar();

                        // Return true if count is greater than 0, indicating a row exists
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"<IsInvoiceHeaderExist> Error: {ex.Message}");
            }

            return false; // Return false if an exception occurs
        }

        public string GetLastestVoucher(string EINVType)
        {
            string query;
            if (EINVType == "01")
            {
                query = "SELECT VoucherNo FROM tblVoucherSettings WHERE DocumentType = 'INV'";
            }
            else
            {
                query = "SELECT VoucherNo FROM tblVoucherSettings WHERE DocumentType = 'CN'";
            }

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, mssqlConnection))
                    {
                        // Set the parameter value

                        // Open the connection
                        mssqlConnection.Open();

                        // Use ExecuteScalar to get the count of matching rows
                        var voucherNo = command.ExecuteScalar();

                        // Return true if count is greater than 0, indicating a row exists
                        return voucherNo.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"<IsInvoiceHeaderExist> Error: {ex.Message}");
            }
        }

        public void UpdateLatestVoucher(string mysoftInvNo, string EINVType)
        {
            string query;

            if (EINVType == "01")
            {
                query = "UPDATE tblVoucherSettings SET VoucherNo = @voucherNo WHERE DocumentType = 'INV'";
            }
            else
            {
                query = "UPDATE tblVoucherSettings SET VoucherNo = @voucherNo WHERE DocumentType = 'CN'";
            }
                

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, mssqlConnection))
                    {
                        // Set the parameter value
                        command.Parameters.AddWithValue("@voucherNo", mysoftInvNo ?? "");

                        // Open the connection
                        mssqlConnection.Open();

                        // Execute the update command
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("VoucherNo updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("No matching record found to update.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"<UpdateLatestVoucher> Error: {ex.Message}");
            }
        }

        public string GetExistedInvNo(string voucher)
        {
            string query = "SELECT H_E_InvoiceNumber FROM E_Invoice_Header WHERE H_BillRefNo = @voucher";

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, mssqlConnection))
                    {
                        // Set the parameter value
                        command.Parameters.AddWithValue("@voucher", voucher ?? "");

                        // Open the connection
                        mssqlConnection.Open();

                        // Use ExecuteScalar to get the count of matching rows
                        var voucherNo = command.ExecuteScalar();

                        // Return true if count is greater than 0, indicating a row exists
                        return voucherNo.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"<IsInvoiceHeaderExist> Error: {ex.Message}");
            }
        }

        public bool CheckExistedInvNo(string invNo, double totalAmount)
        {
            string query = "SELECT H_BillRefNo, H_TotalPayableAmount, BuyerTIN FROM E_Invoice_Header WHERE H_BillRefNo = @voucher";

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, mssqlConnection))
                    {
                        // Set the parameter value
                        command.Parameters.AddWithValue("@voucher", invNo ?? "");

                        // Open the connection
                        mssqlConnection.Open();

                        // Execute the query
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Check if a record exists
                            if (reader.Read())
                            {
                                string invoiceNumber = reader["H_BillRefNo"]?.ToString();
                                string buyerTIN = reader["BuyerTIN"]?.ToString();
                                double invoiceTotalAmount = reader["H_TotalPayableAmount"] != DBNull.Value
                                    ? Convert.ToDouble(reader["H_TotalPayableAmount"])
                                    : 0;

                                // Step 1: Check if BuyerTIN is null or empty
                                if (string.IsNullOrEmpty(buyerTIN))
                                {
                                    return false; // BuyerTIN is null or empty
                                }

                                // Step 2: Check if total amount matches
                                if (invoiceTotalAmount == totalAmount)
                                {
                                    return true; // Total amount matches
                                }
                                else
                                {
                                    return false; // Total amount does not match
                                }
                            }
                        }
                    }
                }

                // Return false if no matching record is found
                return false;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"<CheckExistedInvNo> Error: {ex.Message}");
                return false; // Return false in case of an error
            }
        }

        public string GetExistedInvNoUUID(string voucher)
        {
            string query = "SELECT eInv_UUID FROM E_Invoice_Header WHERE H_BillRefNo = @voucher";

            try
            {
                using (SqlConnection mssqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, mssqlConnection))
                    {
                        // Set the parameter value
                        command.Parameters.AddWithValue("@voucher", voucher ?? "");

                        // Open the connection
                        mssqlConnection.Open();

                        // Use ExecuteScalar to get the count of matching rows
                        var invUUID = command.ExecuteScalar();

                        // Return true if count is greater than 0, indicating a row exists
                        return invUUID != null ? invUUID.ToString() : string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                return ($"<IsInvoiceHeaderExist> Error: {ex.Message}");
            }
        }


    }
}
