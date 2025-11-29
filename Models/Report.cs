using System;
using System.Collections.Generic;

namespace BillingSoftware.Models
{
    public class Report
    {
        public string Title { get; set; } = "";
        public string Type { get; set; } = "";
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public List<ReportItem> Items { get; set; } = new List<ReportItem>();
        public decimal TotalAmount { get; set; }
        public int TotalRecords { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        public Report()
        {
            Items = new List<ReportItem>();
            GeneratedDate = DateTime.Now;
        }
    }

    public class ReportItem
    {
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Reference { get; set; } = "";
    }
}