using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncTool
{
    public partial class Form1 : Form
    {
        fbExternal fbExternal = new fbExternal();

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnSync_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStartDate.Value.Date;
            fbExternal.Fetchdata(startDate);

            // Await the async method
            List<string> logMessages = await fbExternal.CheckOpenStatus(startDate);

            if (logMessages.Count == 0)
            {
                // If no records found, display a message box
                tbMessage.Text = "";
                MessageBox.Show("No invoices with OPEN status found since " + startDate.ToString("yyyy-MM-dd"), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var messageText = string.Join(Environment.NewLine, logMessages);

                // Set the TextBox text in one operation
                tbMessage.Text = messageText;

                await fbExternal.LogOpenMessages(logMessages, startDate);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Last Record has been fetch!","Fetch Last Record", MessageBoxButtons.OK , MessageBoxIcon.None);
        }

        private void btnUpdateCustomer_Click(object sender, EventArgs e)
        {
            fbExternal.updateCustomer();
            MessageBox.Show("Customer Update Successfully!", "Update Customer", MessageBoxButtons.OK, MessageBoxIcon.None);

        }

        private void btnUpdateProduct_Click(object sender, EventArgs e)
        {
            fbExternal.updateProduct();
            MessageBox.Show("Product Update Successfully!", "Update Product", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private async void btnCheck_Click(object sender, EventArgs e)
        {
            DateTime checkDate = dtpCheck.Value.Date;
            
            // Await the async method
            List<string> logMessages = await fbExternal.CheckOpenStatus(checkDate);

            if (logMessages.Count == 0)
            {
                // If no records found, display a message box
                tbMessage.Text = "";
                MessageBox.Show("No invoices with OPEN status found since " + checkDate.ToString("yyyy-MM-dd"), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {

                var messageText = string.Join(Environment.NewLine, logMessages);

                // Set the TextBox text in one operation
                tbMessage.Text = messageText;

                await fbExternal.LogOpenMessages(logMessages, checkDate);
            }
        }
    }
}
