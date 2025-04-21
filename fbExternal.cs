using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SyncTool
{
    internal class fbExternal
    {
        private string fbConnectionString; 

        // Initial Connection String 
        public fbExternal()
        {
            //Initial to get the connection String
            fbConnectionString = FBConnectionStringFromJson();
        }

        // Function to get Firebird and Mssql Conection String from Json
        private string FBConnectionStringFromJson()
        {
            string jsonFilePath = "ConnectionString.json";
            string connectionStringName = "FirebirdConnection";

            try
            {
                string jsonString = File.ReadAllText(jsonFilePath);
                JsonDocument doc = JsonDocument.Parse(jsonString);

                string connectionString = doc.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty(connectionStringName)
                    .GetString();

                Console.WriteLine($"Connection string read successfully: {connectionString}");
                return connectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading connection string: {ex.Message}");
                return null;
            }
        }

        // Check Connection string is Correct
        public void Fetchdata(DateTime startDate)
        {
            //Check Connection String is correct before start fetch Data
            try
            {
                using (FbConnection connection = new FbConnection(fbConnectionString))
                {
                    connection.Open();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                MessageBox.Show("Connection Fail Pls Check Connection String \n" + ex.Message, "Data Connection - Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            getDataFromfbExternal(startDate);
        }

        // Get Customer From Firebired and update to mssql
        public void updateCustomer()
        {
            mssqlInternal mssqlInternal = new mssqlInternal();
            Customer customer = new Customer();
            using (FbConnection fbConnection = new FbConnection(fbConnectionString))
            {
                fbConnection.Open();
                string fbSelectItemQuery = "SELECT CODE, NAME, SEX, CARDNO, ICNO, EMAIL, ADDRESS1, ADDRESS2, ADDRESS3, POSTCODE, TEL1, MOBILE FROM DBMEMBER";

                using (FbCommand fbSelectItemCommand = new FbCommand(fbSelectItemQuery, fbConnection))
                {
                    try
                    {
                        using (FbDataReader customerReader = fbSelectItemCommand.ExecuteReader())
                        {
                            while (customerReader.Read())
                            {
                                customer.BuyerCode = customerReader["CODE"].ToString();
                                customer.BuyerName = customerReader["NAME"].ToString();
                                customer.BuyerTin = customerReader["CODE"].ToString();

                                if (customerReader["SEX"].ToString() == "C")
                                {
                                    customer.BuyerIDType = "Company No.(New)";
                                }
                                else
                                {
                                    customer.BuyerIDType = "IC No.";
                                }

                                customer.BuyerID = customerReader["CARDNO"].ToString();
                                customer.BuyerSST = customerReader["ICNO"].ToString();
                                customer.BuyerEmail = customerReader["EMAIL"].ToString();
                                customer.BuyerAddress0 = customerReader["ADDRESS1"].ToString();
                                customer.BuyerAddress1 = customerReader["ADDRESS2"].ToString();
                                customer.BuyerAddress2 = customerReader["ADDRESS3"].ToString();
                                customer.BuyerPostal = customerReader["POSTCODE"].ToString();
                                customer.BuyerCity = "-";
                                customer.BuyerState = "00";
                                customer.BuyerCountry = "MYR";

                                if (string.IsNullOrEmpty(customerReader["TEL1"].ToString()))
                                {
                                    customer.BuyerContactNo = customerReader["MOBILE"].ToString();
                                }
                                else
                                {
                                    customer.BuyerContactNo = customerReader["TEL1"].ToString();
                                }

                                bool isExist =  mssqlInternal.CheckCustomerCodeExists(customer.BuyerCode);

                                if (isExist)
                                {
                                    mssqlInternal.updateCustomer(customer);
                                }
                                else
                                {
                                    mssqlInternal.insertCustomer(customer);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        using (StreamWriter writer = new StreamWriter("log.txt", true))
                        {
                            writer.WriteLine($"[{DateTime.Now}] <updateCustomer> Error : {ex.Message} ");
                        }
                    }
                }
            }
        }

        // Get Product From Firebired and update to mssql
        public void updateProduct()
        {
            mssqlInternal mssqlInternal = new mssqlInternal();
            Product product = new Product();
            using (FbConnection fbConnection = new FbConnection(fbConnectionString))
            {
                fbConnection.Open();
                string fbSelectItemQuery = "SELECT DBPRODUCT.CODE,DBPRODUCT.DESCRIPTION, DBPRICESTRUCTURE.PRICE FROM DBPRODUCT LEFT JOIN DBPRICESTRUCTURE ON DBPRODUCT.CODE = DBPRICESTRUCTURE.PRODUCTCODE";

                using (FbCommand fbSelectItemCommand = new FbCommand(fbSelectItemQuery, fbConnection))
                {
                    try
                    {
                        using (FbDataReader productReader = fbSelectItemCommand.ExecuteReader())
                        {
                            while (productReader.Read())
                            {
                                product.ProductCode = productReader["CODE"].ToString();
                                product.ProductDescription = productReader["DESCRIPTION"].ToString();
                                double price = double.Parse(productReader["PRICE"].ToString()) * 1.18;
                                product.ProductPrice = price.ToString();

                                List<string> list = new List<string>();
                                bool isExist = mssqlInternal.CheckProductCodeExists(product.ProductCode);

                                if (isExist)
                                {
                                    mssqlInternal.updateProduct(product);
                                }
                                else
                                {
                                    mssqlInternal.insertProduct(product);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        using (StreamWriter writer = new StreamWriter("log.txt", true))
                        {
                            writer.WriteLine($"[{DateTime.Now}] <updateCustomer> Error : {ex.Message} ");
                        }
                    }
                }
            }
        }

        // Get Invoice From Firebired and update to mssql
        public async void getDataFromfbExternal(DateTime startDate)
        {
            using (var fbConnection = new FbConnection(fbConnectionString))
            {
                await fbConnection.OpenAsync();

                //asign the class to get fucntion relate to MsSql Server
                mssqlInternal mssqlInternal = new mssqlInternal();

                //Get Company profile from the MsSql Server
                CompanyProfile companyDetail = mssqlInternal.getSupplierDetails();
                
                // Step 1: Fetch data from Firebird (fbInvoiceHeader, fbInvoiceDetail, fbMember, fbProduct) where status = closed
                var fbInvoices = await getFBInvoiceHeader(startDate, fbConnection);
                foreach (var fbInvoice in fbInvoices)
                {

                    // Step 2: Check if there are any details with cRemark containing an invoice number
                    var invoiceDetails = await getFBInvoiceDetails(fbInvoice.fbInvoiceNumber, fbConnection);

                    bool shouldProcessTrans = true;
                    string CInvNo = "NA";

                    foreach (var invoiceDetail in invoiceDetails)
                    {
                        // If the CRemark is null or blank, just process as invoice logic, don't check CN conditions
                        if (string.IsNullOrEmpty(invoiceDetail.CRemark))
                        {
                            continue;
                        }

                        string CaptureInvNo = invoiceDetail.CRemark.Split(' ')[0];

                        CInvNo = Regex.Replace(CaptureInvNo, @"\D", ""); // Replaces all non-digit characters with an empty string

                        if (!string.IsNullOrEmpty(invoiceDetail.CRemark) && IsValidInvoiceNumber(CInvNo) && string.IsNullOrEmpty(mssqlInternal.GetExistedInvNoUUID(CInvNo)))
                        {
                            // If cRemark contains a valid invoice number, mark this for continue procees
                            shouldProcessTrans = false;
                            break;
                        }
                    }

                    if (!shouldProcessTrans)
                    {
                        continue; // Skip processing this invoice since we only needed to ignore
                    }

                    // Step 4: Retrieve and map buyerInfo, and invoice details
                    var buyerInfo =  await GetBuyerInfoFromFbMember(fbInvoice.fbMemberCode, fbConnection);

                    // Step 5: Insert or update mssqlInvoiceHeader and mssqlInvoiceDetail
                    InsertOrUpdateEInvoice(fbInvoice, companyDetail, buyerInfo, invoiceDetails,CInvNo);
                }
            }

            MessageBox.Show("Sync Done!");

        }

        // Get Invoice Header from Firebird
        public async Task<List<FbInvoiceHeader>> getFBInvoiceHeader(DateTime startDate, FbConnection fbConnection)
        {
            var query = "SELECT CASHNO, CASHDATE, GROSSSALES, TOTALTAX1, TOTALTAX2, DISCOUNT, TOTALSALES, STATUS, MEMBERCODE, TS FROM DBFBHDR WHERE CASHDATE >= @startDate AND STATUS='CLOSED'";

            var fbInvoices = new List<FbInvoiceHeader>();
            
            using (var fbCommand = new FbCommand(query, fbConnection))
            {
                // Parameterize the query to prevent SQL injection
                fbCommand.Parameters.AddWithValue("@startDate", startDate);

                // Execute the query asynchronously
                using (var reader = await fbCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        fbInvoices.Add(new FbInvoiceHeader
                        {
                            fbInvoiceNumber = reader["CASHNO"].ToString(),
                            fbInvoiceDate = reader.GetDateTime(reader.GetOrdinal("CASHDATE")).ToString("dd/MM/yyyy"),
                            fbInvoiceTime = reader.GetDateTime(reader.GetOrdinal("CASHDATE")).ToString("HH:mm:ss"),
                            fbGrossSales = double.TryParse(reader["GROSSSALES"].ToString(), out double grossSales) ? grossSales : 0,
                            fbTotalChargeAmount = double.TryParse(reader["TOTALTAX1"].ToString(), out double totalChargeAmount) ? totalChargeAmount : 0,
                            fbTotalTaxAmount = double.TryParse(reader["TOTALTAX2"].ToString(), out double totalTaxAmount) ? totalTaxAmount : 0,
                            fbDiscount = double.TryParse(reader["DISCOUNT"].ToString(), out double discount) ? Math.Abs(discount) : 0, // Ensure discount is positive
                            fbTotalSales = double.TryParse(reader["TOTALSALES"].ToString(), out double totalSales) ? totalSales : 0,
                            fbStatus = reader["STATUS"].ToString(),
                            fbMemberCode = reader["MEMBERCODE"].ToString(),
                            fbMysoftInv = "" // To get increment number
                        }) ;
                    }
                }
            }

            return fbInvoices;
        }

        // Get Invoice Detail from Firebird
        public async Task<List<FbInvoiceDetail>> getFBInvoiceDetails(string InvoiceNumber, FbConnection fbConnection)
        {
            var query = "SELECT DTL.PRODUCTCODE, DTL.PRICE, DTL.SUBTOTAL, DTL.DISC, DTL.QTY, DTL.TAX1, DTL.TAX2, DTL.CREMARKS, PD.TAX1 as chargeRate, PD.TAX2 as taxRate, PD.DESCRIPTION FROM DBFBDTL DTL JOIN DBPRODUCT PD ON DTL.PRODUCTCODE = PD.CODE WHERE DTL.CASHNO = @InvoiceNo";

            var fbInvoicesDetails = new List<FbInvoiceDetail>();

            int lineNo = 1;

            using (var fbCommand = new FbCommand(query, fbConnection))
            {
                // Parameterize the query to prevent SQL injection
                fbCommand.Parameters.AddWithValue("@InvoiceNo", InvoiceNumber);

                // Execute the query asynchronously
                using (var reader = await fbCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        fbInvoicesDetails.Add(new FbInvoiceDetail
                        {
                            lineNo = lineNo++,
                            productDescription = reader["DESCRIPTION"].ToString(),
                            unitPrice = double.TryParse(reader["PRICE"].ToString(), out double unitPriceValue) ? unitPriceValue : 0,
                            taxRate = double.TryParse(reader["taxRate"].ToString(), out double taxRateValue) ? taxRateValue : 0,
                            taxAmount = double.TryParse(reader["TAX2"].ToString(), out double taxAmountValue) ? taxAmountValue : 0,
                            subtotal = double.TryParse(reader["SUBTOTAL"].ToString(), out double subtotalValue) ? subtotalValue : 0,
                            discount = double.TryParse(reader["DISC"].ToString(), out double discountValue) ? Math.Abs(discountValue) : 0,
                            quantity = double.TryParse(reader["QTY"].ToString(), out double QuantityValue) ? QuantityValue : 0,
                            chargeRate = double.TryParse(reader["chargeRate"].ToString(), out double chargeRateValue) ? chargeRateValue : 0,
                            chargeAmount = double.TryParse(reader["TAX1"].ToString(), out double chargeAmountValue) ? chargeAmountValue : 0,
                            CRemark = reader["CREMARKS"].ToString(),
                            unitOfMeasurement = "",
                            productCode = reader["PRODUCTCODE"].ToString(),
                        });
                    }
                }
            }

            return fbInvoicesDetails;
        }

        // Get Invoice Buyer Info from Firebird
        public async Task<FbBuyerInfo> GetBuyerInfoFromFbMember(string memberCode, FbConnection fbConnection)
        {
            var query = "SELECT CODE, NAME, ICNO, ADDRESS1, ADDRESS2, ADDRESS3, POSTCODE, MOBILE, SEX, EMAIL, CARDNO FROM DBMEMBER WHERE CODE = @MemberCode";

            var fbBuyer = new FbBuyerInfo();

            using (var fbCommand = new FbCommand(query, fbConnection))
            {
                // Parameterize the query to prevent SQL injection
                fbCommand.Parameters.AddWithValue("@MemberCode", memberCode);

                // Execute the query asynchronously
                using (var reader = await fbCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) // Read the first matching result
                    {
                        string idNumber;

                        // Null check for CARDNO and handle sex-based formatting
                        if (reader["SEX"].ToString() == "C")
                        {
                            idNumber = reader["CARDNO"] != DBNull.Value ? "BRN:" + reader["CARDNO"].ToString() : "BRN:N/A";
                        }
                        else
                        {
                            idNumber = reader["CARDNO"] != DBNull.Value ? "NRIC:" + reader["CARDNO"].ToString() : "NRIC:N/A";
                        }

                        // Assign values to fbBuyer
                        fbBuyer = new FbBuyerInfo
                        {
                            Name = string.IsNullOrWhiteSpace(reader["NAME"].ToString()) ? "" : reader["NAME"].ToString(),
                            IdNumber = string.IsNullOrWhiteSpace(idNumber) ? "N/A" : idNumber,
                            TinNumber = string.IsNullOrWhiteSpace(reader["CODE"].ToString()) ? "" : reader["CODE"].ToString(),
                            Address1 = string.IsNullOrWhiteSpace(reader["ADDRESS1"].ToString()) ? "" : reader["ADDRESS1"].ToString(),
                            Address2 = string.IsNullOrWhiteSpace(reader["ADDRESS2"].ToString()) ? "" : reader["ADDRESS2"].ToString(),
                            Address3 = string.IsNullOrWhiteSpace(reader["ADDRESS3"].ToString()) ? "" : reader["ADDRESS3"].ToString(),
                            Email = string.IsNullOrWhiteSpace(reader["EMAIL"].ToString()) ? "" : reader["EMAIL"].ToString(),
                            Postal = string.IsNullOrWhiteSpace(reader["POSTCODE"].ToString()) ? "" : reader["POSTCODE"].ToString(),
                            CityName = "N/A",   // Hardcoded, change if needed
                            State = "00",       // Hardcoded, change if needed
                            Country = "MYS",    // Hardcoded, change if needed
                            Phone = string.IsNullOrWhiteSpace(reader["MOBILE"].ToString()) ? "" : reader["MOBILE"].ToString(),
                            SST = "N/A"         // Hardcoded, change if needed
                        };
                    }
                }
            }

            return fbBuyer; // Return the FbBuyerInfo object (or null if no data was found)
        }

        // check the parameter is a valid invoice number
        public bool IsValidInvoiceNumber(string remark)
        {
            // Check if the remark is not null or whitespace and consists of only numeric characters
            return !string.IsNullOrWhiteSpace(remark) && long.TryParse(remark, out _);
        }

        // To log Close Status list
        public async Task LogOpenMessages(List<string> messages, DateTime checkDate)
        {
            // Ensure log folder exists
            string logFolder = "Close_log";
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            // Generate the log file name with the current date
            string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
            string logFilePath = Path.Combine(logFolder, $"{todayDate}_log.txt");

            try
            {
                // Append new messages to today's log file
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    foreach (var message in messages)
                    {
                        writer.WriteLine($"[{checkDate}] {message}");  // Write each message with timestamp
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while logging messages: {ex.Message}");
            }
        }

        // Check and show all the Close Status by date given
        public async Task<List<string>> CheckOpenStatus(DateTime startDate)
        {
            var query = "SELECT CASHNO FROM DBFBHDR WHERE CASHDATE >= @startDate AND STATUS='OPEN'";

            List<string> logMessages = new List<string>();  // List to store messages for logging

            using (var fbConnection = new FbConnection(fbConnectionString))
            {
                await fbConnection.OpenAsync();  // Ensure connection is opened asynchronously

                using (var fbCommand = new FbCommand(query, fbConnection))
                {
                    // Parameterize the query to prevent SQL injection
                    fbCommand.Parameters.AddWithValue("@startDate", startDate);

                    // Execute the query asynchronously
                    using (var reader = await fbCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Add the CASHNO as a string to the logMessages list
                            logMessages.Add(reader["CASHNO"].ToString());
                        }
                    }
                }
            }
            return logMessages;
        }


        // Combine data than Insert or Update Invoice to mssql
        public async void InsertOrUpdateEInvoice(FbInvoiceHeader InvoiceHeader, CompanyProfile CompanyProfile, FbBuyerInfo BuyerInfo, List<FbInvoiceDetail> InvoiceDetails, string CInvNo)
        {
            E_Invoice_Header header = new E_Invoice_Header();
            E_Invoice_Detail detail = new E_Invoice_Detail();
            mssqlInternal mssqlInternal = new mssqlInternal();
            string mysoftVoucher;
            bool skipUpdate;

            header.SupplierName = CompanyProfile.SupplierName;
            header.SupplierTIN = CompanyProfile.SupplierTIN;
            header.SupplierID = "BRN:"+CompanyProfile.SupplierID;
            header.SupplierSST = CompanyProfile.SupplierSST;
            header.SupplierTourismTax = CompanyProfile.SupplierTourismTax;
            header.SupplierEmail = CompanyProfile.SupplierEmail;
            header.SupplierMSIC = CompanyProfile.SupplierMSIC;
            header.SupplierBusinessActivityDesc = CompanyProfile.SupplierBusinessActivityDesc;
            header.SupplierAddress0 = CompanyProfile.SupplierAddress0;
            header.SupplierAddress1 = CompanyProfile.SupplierAddress1;
            header.SupplierAddress2 = CompanyProfile.SupplierAddress2;
            header.SupplierPostal = CompanyProfile.SupplierPostal;
            header.BuyerName = BuyerInfo.Name;
            header.BuyerTIN = BuyerInfo.TinNumber;
            header.SupplierCity = CompanyProfile.SupplierCity;
            header.SupplierState = CompanyProfile.SupplierState;
            header.SupplierCountry = CompanyProfile.SupplierCountry;
            header.SupplierContactNo = CompanyProfile.SupplierContactNo;
            
            header.BuyerID = BuyerInfo.IdNumber;
            header.BuyerSST = BuyerInfo.SST;
            header.BuyerEmail = BuyerInfo.Email;
            header.BuyerAddress0 = BuyerInfo.Address1;
            header.BuyerAddress1 = BuyerInfo.Address2;
            header.BuyerAddress2 = BuyerInfo.Address3;
            header.BuyerPostal = BuyerInfo.Postal ?? "";
            header.BuyerCity = BuyerInfo.CityName;
            header.BuyerState = BuyerInfo.State;
            header.BuyerCountry = BuyerInfo.Country;
            header.BuyerContactNo = BuyerInfo.Phone;

            header.H_E_InvoiceVersion = "1.0";

            if(InvoiceHeader.fbTotalSales < 0)
            {
                header.H_E_InvoiceType = "02";
            }
            else
            {
                header.H_E_InvoiceType = "01";
            }

            // Combine fbInvoiceDate and fbInvoiceTime into a single DateTime
            DateTime originalDateTime = DateTime.Parse($"{InvoiceHeader.fbInvoiceDate} {InvoiceHeader.fbInvoiceTime}");

            // Subtract 8 hours
            DateTime adjustedDateTime = originalDateTime.AddHours(-8);

            // Separate back into date and time
            header.H_E_InvoiceDate = adjustedDateTime.ToString("yyyy-MM-dd"); // Format as date
            header.H_E_InvoiceTime = adjustedDateTime.ToString("HH:mm:ss");   // Format as time

            header.H_E_InvoiceNumber = InvoiceHeader.fbInvoiceNumber;
            header.H_E_InvoiceDate = InvoiceHeader.fbInvoiceDate;
            header.H_E_InvoiceTime = InvoiceHeader.fbInvoiceTime;
            header.H_SupplierDigitalSignature = "";
            header.H_InvoiceCurCode = "MYR";
            header.H_ExchangeRate = 1.0;
            header.H_FrequencyBilling = "";
            header.H_BillingPeriodStartDate = "";
            header.H_BillingPeriodEndDate = "";
            header.H_PaymentMode = "";
            header.H_SupplierBankAC = "";
            header.H_PaymentTerms = "";
            header.H_PrePaymentAmount = "";
            header.H_PrePaymentDate = "";
            header.H_PrePaymentTime = "";
            header.H_PrePaymentRefNo = "";
            header.H_BillRefNo = InvoiceHeader.fbInvoiceNumber;
            header.H_TotalExcludingTax = Math.Abs(InvoiceHeader.fbGrossSales + InvoiceHeader.fbTotalChargeAmount - InvoiceHeader.fbDiscount);
            header.H_TotalIncludingTax = Math.Abs(InvoiceHeader.fbGrossSales + InvoiceHeader.fbTotalChargeAmount - InvoiceHeader.fbDiscount + InvoiceHeader.fbTotalTaxAmount);
            header.H_TotalPayableAmount = Math.Abs(InvoiceHeader.fbTotalSales);
            header.H_TotalNetAmount = 0;
            header.H_TotalDiscountValue = Math.Abs(InvoiceHeader.fbDiscount);
            header.H_TotalChargeAmount = Math.Abs(InvoiceHeader.fbTotalChargeAmount);
            header.H_TotalTaxAmount = Math.Abs(InvoiceHeader.fbTotalTaxAmount);
            header.H_RoundingAmount = 0;
            header.H_TotalTaxAmtPerTaxType = Math.Abs(InvoiceHeader.fbTotalTaxAmount);
            header.H_TaxType = "02";
            header.H_AdditionalDiscountAmount = 0;
            header.H_AdditionalFeeAmount = 0;
            header.H_ShippingName = "";
            header.H_ShippingAddress0 = "";
            header.H_ShippingAddress1 = "";
            header.H_ShippingAddress2 = "";
            header.H_ShippingPostal = "";
            header.H_ShippingCity = "";
            header.H_ShippingState = "";
            header.H_ShippingCountry = "";
            header.H_ShippingTIN = "";
            header.H_ShippingID = "";
            header.H_CustomForm1to9RefNo = "";
            header.H_Incoterms = "";
            header.H_FTAInfo = "";
            header.H_CertifiedExporterAuthNo = "";
            header.H_CustomForm2 = "";
            header.H_OtherChargesDetail = "";
            header.MySoft_ARAPCode = "";
            header.MySoft_IsSelfBill = false;
            header.MySoft_UserName = "Manager";
            header.MySoft_LogDate = DateTime.Now;
            header.H_TaxExemption = "";
            header.H_TaxExemptionAmount = "";
            header.eInv_isSubmit = true;

            if (header.H_E_InvoiceType == "02")
            {
                header.H_OriginalDocUUID = mssqlInternal.GetExistedInvNoUUID(CInvNo);
                header.H_OriginalDocNo = CInvNo;
            }
            else
            {
                header.H_OriginalDocUUID = "";
                header.H_OriginalDocNo = "";
            }
            

            if (mssqlInternal.isInvoiceHeaderExist(InvoiceHeader.fbInvoiceNumber))
            {
                mysoftVoucher = mssqlInternal.GetExistedInvNo(InvoiceHeader.fbInvoiceNumber);
                skipUpdate = mssqlInternal.CheckExistedInvNo(InvoiceHeader.fbInvoiceNumber, InvoiceHeader.fbTotalSales);
                if(!skipUpdate)
                {
                    mssqlInternal.updateEinvoiceHeader(header);
                }
            }
            else {
                skipUpdate = false;
                mysoftVoucher = mssqlInternal.GetLastestVoucher(header.H_E_InvoiceType);
                mysoftVoucher = GetNextVoucherNo(mysoftVoucher);
                header.H_E_InvoiceNumber = mysoftVoucher;
                mssqlInternal.insertEinvoiceHeader(header);
                mssqlInternal.UpdateLatestVoucher(mysoftVoucher, header.H_E_InvoiceType);
            }

            if (!skipUpdate)
            {
                //mssqlInternal.DeleteInvoiceDetail(InvoiceHeader.fbInvoiceNumber);
                mssqlInternal.DeleteInvoiceDetail(InvoiceHeader.fbInvoiceNumber);

                foreach (var invoiceDetail in InvoiceDetails)
                {
                    detail.H_E_InvoiceNumber = mysoftVoucher;
                    detail.SupplierTIN = CompanyProfile.SupplierTIN;
                    detail.BuyerTIN = BuyerInfo.TinNumber;
                    detail.LI_ProductClassification = await mssqlInternal.GetProductClassificationAsync(invoiceDetail.productCode);
                    detail.LI_ProductDescription = invoiceDetail.productDescription;
                    detail.LI_UnitPrice = invoiceDetail.unitPrice;
                    detail.LI_TaxType = "02";
                    detail.LI_TaxRate = invoiceDetail.taxRate;
                    detail.LI_TaxAmount = Math.Abs(invoiceDetail.taxAmount);
                    detail.LI_TaxExemption = "";//if applicable
                    detail.LI_TaxExemptionAmount = 0;//if applicable
                    detail.LI_SubTotal = Math.Abs(invoiceDetail.subtotal);
                    detail.LI_TotalExcludingTax = Math.Abs(invoiceDetail.subtotal + invoiceDetail.chargeAmount - invoiceDetail.discount);
                    detail.LI_Quantity = invoiceDetail.quantity;
                    detail.LI_UOM = "";//optional
                    detail.LI_DiscountRate = 0;//optional
                    detail.LI_DiscountAmount = Math.Abs(invoiceDetail.discount);
                    detail.LI_ChargeRate = invoiceDetail.chargeRate;
                    detail.LI_ChargeAmount = Math.Abs(invoiceDetail.chargeAmount);
                    detail.LI_ProductTariffCode = "";
                    detail.LI_CountryOrigin = "";
                    detail.MySoft_StockCode = invoiceDetail.productCode;

                    mssqlInternal.insertEinvoiceItem(detail);
                }
            }
        }

        public static string GetNextVoucherNo(string voucherNo)
        {
            // Regular expression to match prefix (letters and numbers) and numeric part
            var match = Regex.Match(voucherNo, @"^([^\d]+)(\d+\/)?(\d+)$");

            if (match.Success)
            {
                // Prefix includes the part before the main number
                string prefix = match.Groups[1].Value + match.Groups[2].Value;
                string numberPart = match.Groups[3].Value;  // Extract numeric part (e.g., "000001")

                // Increment the numeric part
                int number = int.Parse(numberPart) + 1;

                // Format the new number with leading zeros to match the original length
                string newNumberPart = number.ToString().PadLeft(numberPart.Length, '0');

                // Combine prefix and incremented number
                return prefix + newNumberPart;
            }
            else
            {
                throw new ArgumentException("Invalid VoucherNo format");
            }
        }

    }

}
