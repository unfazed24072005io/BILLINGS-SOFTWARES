using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Data;
using System.Collections.Generic;
using BillingSoftware.Models;

namespace BillingSoftware.Utilities
{
    public class PrintHelper
    {
        private PrintDocument printDocument;
        private DataTable dataTable;
        private string title;
        private Font printFont;
        private int currentRow = 0;
        private float yPos = 0;
        private float leftMargin = 50;
        private float topMargin = 50;
        
        // For voucher printing
        private Voucher voucher;
        private List<VoucherItem> voucherItems;
        private decimal discountPercent;
        private decimal taxPercent;
        
        public PrintHelper()
        {
            printDocument = new PrintDocument();
            printDocument.PrintPage += new PrintPageEventHandler(PrintPageHandler);
            printFont = new Font("Arial", 10);
        }
        
        // Print DataTable (for reports)
        public void PrintDataTable(DataTable data, string reportTitle)
        {
            dataTable = data;
            title = reportTitle;
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;
            
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }
        
        // Print Voucher
        public void PrintVoucher(Voucher voucherToPrint, List<VoucherItem> items)
        {
            voucher = voucherToPrint;
            voucherItems = items;
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;
            
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }
        
        // Print Estimate with Tax & Discount
        public void PrintEstimate(Voucher estimate, List<VoucherItem> items, decimal discount, decimal tax)
        {
            voucher = estimate;
            voucherItems = items;
            discountPercent = discount;
            taxPercent = tax;
            
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;
            
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }
        
        private void PrintPageHandler(object sender, PrintPageEventArgs ev)
        {
            yPos = topMargin;
            
            // Print company header
            PrintCompanyHeader(ev);
            
            if (voucher != null)
            {
                PrintVoucherContent(ev);
            }
            else if (dataTable != null)
            {
                PrintReportContent(ev);
            }
            
            ev.HasMorePages = false;
        }
        
        private void PrintCompanyHeader(PrintPageEventArgs ev)
        {
            Font headerFont = new Font("Arial", 16, FontStyle.Bold);
            Font subHeaderFont = new Font("Arial", 12);
            
            // Company Name
            ev.Graphics.DrawString("MY BILLING COMPANY", headerFont, Brushes.Black, 
                                  leftMargin, yPos);
            yPos += headerFont.GetHeight() + 10;
            
            // Address
            ev.Graphics.DrawString("123 Business Street, City - 123456", subHeaderFont, 
                                  Brushes.Black, leftMargin, yPos);
            yPos += subHeaderFont.GetHeight() + 5;
            
            ev.Graphics.DrawString("Phone: 9876543210 | GSTIN: 27ABCDE1234F1Z5", 
                                  subHeaderFont, Brushes.Black, leftMargin, yPos);
            yPos += subHeaderFont.GetHeight() + 20;
            
            // Title line
            if (!string.IsNullOrEmpty(title))
            {
                ev.Graphics.DrawString(title, new Font("Arial", 14, FontStyle.Bold), 
                                      Brushes.Black, leftMargin, yPos);
                yPos += 30;
            }
        }
        
        private void PrintVoucherContent(PrintPageEventArgs ev)
        {
            Font titleFont = new Font("Arial", 14, FontStyle.Bold);
            Font normalFont = new Font("Arial", 10);
            Font boldFont = new Font("Arial", 10, FontStyle.Bold);
            
            // Voucher Title
            string voucherTitle = $"{voucher.Type.ToUpper()} VOUCHER";
            ev.Graphics.DrawString(voucherTitle, titleFont, Brushes.Black, 
                                  leftMargin, yPos);
            yPos += titleFont.GetHeight() + 10;
            
            // Voucher Details
            ev.Graphics.DrawString($"Voucher No: {voucher.Number}", normalFont, 
                                  Brushes.Black, leftMargin, yPos);
            ev.Graphics.DrawString($"Date: {voucher.Date:dd-MMM-yyyy}", normalFont, 
                                  Brushes.Black, 300, yPos);
            yPos += normalFont.GetHeight() + 5;
            
            ev.Graphics.DrawString($"Party: {voucher.Party}", normalFont, 
                                  Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight() + 15;
            
            // Items Header
            ev.Graphics.DrawString("----------------------------------------------------------", 
                                  normalFont, Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            string itemsHeader = "Sr.  Description               Qty   Rate    Amount";
            ev.Graphics.DrawString(itemsHeader, boldFont, Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            ev.Graphics.DrawString("----------------------------------------------------------", 
                                  normalFont, Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            // Items
            int srNo = 1;
            decimal subtotal = 0;
            
            foreach (var item in voucherItems)
            {
                string itemLine = $"{srNo,-4} {item.ProductName,-25} {item.Quantity,5} {item.UnitPrice,8:N2} {item.TotalAmount,10:N2}";
                ev.Graphics.DrawString(itemLine, normalFont, Brushes.Black, leftMargin, yPos);
                yPos += normalFont.GetHeight();
                subtotal += item.TotalAmount;
                srNo++;
                
                if (yPos > ev.MarginBounds.Height - 150)
                {
                    ev.HasMorePages = true;
                    return;
                }
            }
            
            ev.Graphics.DrawString("----------------------------------------------------------", 
                                  normalFont, Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            // Subtotal
            ev.Graphics.DrawString($"Sub Total: {subtotal,40:N2}", boldFont, 
                                  Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            // For estimates - show discount and tax
            if (voucher.Type == "Estimate" && discountPercent > 0)
            {
                decimal discountAmount = subtotal * (discountPercent / 100);
                ev.Graphics.DrawString($"Discount ({discountPercent}%): {discountAmount,32:N2}", 
                                      normalFont, Brushes.Black, leftMargin, yPos);
                yPos += normalFont.GetHeight();
                subtotal -= discountAmount;
            }
            
            if (voucher.Type == "Estimate" && taxPercent > 0)
            {
                decimal taxAmount = subtotal * (taxPercent / 100);
                ev.Graphics.DrawString($"Tax ({taxPercent}%): {taxAmount,38:N2}", 
                                      normalFont, Brushes.Black, leftMargin, yPos);
                yPos += normalFont.GetHeight();
                subtotal += taxAmount;
            }
            
            // Grand Total
            ev.Graphics.DrawString($"Grand Total: {voucher.Amount,38:N2}", 
                                  new Font("Arial", 11, FontStyle.Bold), 
                                  Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight() + 10;
            
            // Footer
            ev.Graphics.DrawString("----------------------------------------------------------", 
                                  normalFont, Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            ev.Graphics.DrawString("Thank You for Your Business!", 
                                  new Font("Arial", 10, FontStyle.Italic), 
                                  Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            ev.Graphics.DrawString($"Printed on: {DateTime.Now:dd-MMM-yyyy HH:mm}", 
                                  normalFont, Brushes.Black, leftMargin, yPos);
        }
        
        private void PrintReportContent(PrintPageEventArgs ev)
        {
            Font headerFont = new Font("Arial", 11, FontStyle.Bold);
            Font normalFont = new Font("Arial", 10);
            
            // Column Headers
            float[] columnWidths = { 50, 150, 80, 80, 80, 80, 80 };
            float xPos = leftMargin;
            
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                ev.Graphics.DrawString(dataTable.Columns[i].ColumnName, headerFont, 
                                      Brushes.Black, xPos, yPos);
                xPos += columnWidths[i];
            }
            
            yPos += headerFont.GetHeight() + 5;
            
            // Data Rows
            for (int row = currentRow; row < dataTable.Rows.Count; row++)
            {
                xPos = leftMargin;
                
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    string cellValue = dataTable.Rows[row][col].ToString();
                    ev.Graphics.DrawString(cellValue, normalFont, Brushes.Black, 
                                          xPos, yPos);
                    xPos += columnWidths[col];
                }
                
                yPos += normalFont.GetHeight();
                currentRow++;
                
                if (yPos > ev.MarginBounds.Height - 50)
                {
                    ev.HasMorePages = true;
                    return;
                }
            }
            
            // Footer
            yPos += 20;
            ev.Graphics.DrawString($"Total Records: {dataTable.Rows.Count}", 
                                  normalFont, Brushes.Black, leftMargin, yPos);
            yPos += normalFont.GetHeight();
            
            ev.Graphics.DrawString($"Printed on: {DateTime.Now:dd-MMM-yyyy HH:mm}", 
                                  normalFont, Brushes.Black, leftMargin, yPos);
        }
    }
}