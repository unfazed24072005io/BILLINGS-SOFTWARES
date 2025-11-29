using System;
using System.Collections.Generic;

namespace BillingSoftware.Models
{
    public class Report
    {
        public string Title { get; set; }
        public string Type { get; set; }  // Daily, Monthly, Yearly, Stock, Sales, Financial
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<ReportItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalRecords { get; set; }
        public DateTime GeneratedDate { get; set; }

        public Report()
        {
            Items = new List<ReportItem>();
            GeneratedDate = DateTime.Now;
        }
    }

    public class ReportItem
    {
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Reference { get; set; }
    }
}