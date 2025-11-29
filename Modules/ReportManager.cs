using System;
using System.Collections.Generic;
using System.Linq;
using BillingSoftware.Models;

namespace BillingSoftware.Modules
{
    public class ReportManager
    {
        private DatabaseManager dbManager;
        private VoucherManager voucherManager;

        public ReportManager()
        {
            dbManager = new DatabaseManager();
            voucherManager = new VoucherManager();
        }

        public int GetLowStockCount()
        {
            return 2; // Sample data
        }

        // Daily Stock Report
        public Report GenerateDailyStockReport(DateTime date)
        {
            var report = new Report
            {
                Type = "Daily Stock",
                Title = $"Daily Stock Report - {date:dd-MMM-yyyy}",
                FromDate = date,
                ToDate = date,
                GeneratedDate = DateTime.Now
            };

            var stockData = GetSampleStockMovementData(date);

            foreach (var item in stockData)
            {
                var reportItem = new ReportItem
                {
                    Description = item.ItemName,
                    Quantity = item.OpeningStock,
                    Amount = item.Bought,
                    Date = date,
                    Reference = $"Sold: {item.Sold} | Closing: {item.ClosingStock}"
                };

                report.Items.Add(reportItem);
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Monthly Stock Report
        public Report GenerateMonthlyStockReport(int year, int month)
        {
            var fromDate = new DateTime(year, month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);

            var report = new Report
            {
                Type = "Monthly Stock",
                Title = $"Monthly Stock Report - {fromDate:MMMM yyyy}",
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedDate = DateTime.Now
            };

            var monthlyData = GetSampleMonthlyStockData(year, month);

            foreach (var item in monthlyData)
            {
                var reportItem = new ReportItem
                {
                    Description = item.ItemName,
                    Quantity = item.OpeningStock,
                    Amount = item.Bought,
                    Date = fromDate,
                    Reference = $"Sold: {item.Sold} | Closing: {item.ClosingStock}"
                };

                report.Items.Add(reportItem);
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Yearly Stock Summary
        public Report GenerateYearlyStockReport(int year)
        {
            var report = new Report
            {
                Type = "Yearly Stock",
                Title = $"Yearly Stock Summary - {year}",
                FromDate = new DateTime(year, 1, 1),
                ToDate = new DateTime(year, 12, 31),
                GeneratedDate = DateTime.Now
            };

            var yearlyData = GetSampleYearlyStockData(year);

            foreach (var item in yearlyData)
            {
                var reportItem = new ReportItem
                {
                    Description = item.ItemName,
                    Quantity = item.OpeningStock,
                    Amount = item.Bought,
                    Date = new DateTime(year, 1, 1),
                    Reference = $"Sold: {item.Sold} | Closing: {item.ClosingStock}"
                };

                report.Items.Add(reportItem);
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Sales Report
        public Report GenerateSalesReport(DateTime fromDate, DateTime toDate)
        {
            var report = new Report
            {
                Type = "Sales",
                Title = $"Sales Report ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy})",
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedDate = DateTime.Now
            };

            var sampleSales = new List<Voucher>
            {
                new Voucher { Type = "Sales", Number = "SL-001", Date = DateTime.Now.AddDays(-5), Party = "Customer A", Amount = 15000, Description = "Laptop Sale" },
                new Voucher { Type = "Sales", Number = "SL-002", Date = DateTime.Now.AddDays(-3), Party = "Customer B", Amount = 25000, Description = "Monitor + Accessories" },
                new Voucher { Type = "Sales", Number = "SL-003", Date = DateTime.Now.AddDays(-1), Party = "Customer C", Amount = 8000, Description = "Keyboard & Mouse" },
                new Voucher { Type = "Sales", Number = "SL-004", Date = DateTime.Now, Party = "Customer D", Amount = 50000, Description = "Bulk Order - 2 Laptops" }
            };

            foreach (var sale in sampleSales.Where(s => s.Date >= fromDate && s.Date <= toDate))
            {
                var item = new ReportItem
                {
                    Description = sale.Description,
                    Amount = sale.Amount,
                    Date = sale.Date,
                    Reference = sale.Number
                };

                report.Items.Add(item);
                report.TotalAmount += sale.Amount;
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Financial Report
        public Report GenerateFinancialReport(DateTime fromDate, DateTime toDate)
        {
            var report = new Report
            {
                Type = "Financial",
                Title = $"Financial Summary ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy})",
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedDate = DateTime.Now
            };

            var sampleTransactions = new List<Voucher>
            {
                new Voucher { Type = "Sales", Number = "SL-001", Date = DateTime.Now.AddDays(-10), Party = "Customer A", Amount = 15000 },
                new Voucher { Type = "Sales", Number = "SL-002", Date = DateTime.Now.AddDays(-5), Party = "Customer B", Amount = 25000 },
                new Voucher { Type = "Receipt", Number = "RCPT-001", Date = DateTime.Now.AddDays(-3), Party = "Customer C", Amount = 10000 },
                new Voucher { Type = "Payment", Number = "PAY-001", Date = DateTime.Now.AddDays(-2), Party = "Supplier X", Amount = 8000 },
                new Voucher { Type = "Payment", Number = "PAY-002", Date = DateTime.Now.AddDays(-1), Party = "Supplier Y", Amount = 5000 }
            };

            foreach (var transaction in sampleTransactions.Where(t => t.Date >= fromDate && t.Date <= toDate))
            {
                var item = new ReportItem
                {
                    Description = $"{transaction.Type} - {transaction.Party}",
                    Amount = transaction.Amount,
                    Date = transaction.Date,
                    Reference = transaction.Number
                };

                report.Items.Add(item);
                report.TotalAmount += transaction.Amount;
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Voucher Summary Report
        public Report GenerateVoucherSummary(DateTime fromDate, DateTime toDate)
        {
            var report = new Report
            {
                Type = "Voucher Summary",
                Title = $"Voucher Summary Report ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy})",
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedDate = DateTime.Now
            };

            var sampleVouchers = new List<Voucher>
            {
                new Voucher { Type = "Sales", Number = "SL-001", Date = DateTime.Now.AddDays(-5), Party = "Customer A", Amount = 15000 },
                new Voucher { Type = "Sales", Number = "SL-002", Date = DateTime.Now.AddDays(-3), Party = "Customer B", Amount = 25000 },
                new Voucher { Type = "Receipt", Number = "RCPT-001", Date = DateTime.Now.AddDays(-2), Party = "Customer C", Amount = 10000 },
                new Voucher { Type = "Payment", Number = "PAY-001", Date = DateTime.Now.AddDays(-1), Party = "Supplier X", Amount = 8000 }
            };

            var voucherSummary = sampleVouchers
                .Where(v => v.Date >= fromDate && v.Date <= toDate)
                .GroupBy(v => v.Type)
                .Select(g => new ReportItem
                {
                    Description = $"{g.Key} Vouchers",
                    Quantity = g.Count(),
                    Amount = g.Sum(v => v.Amount),
                    Reference = $"{g.Count()} transactions"
                })
                .ToList();

            report.Items.AddRange(voucherSummary);
            report.TotalAmount = voucherSummary.Sum(item => item.Amount);
            report.TotalRecords = voucherSummary.Count;

            return report;
        }

        // Stock Report (Generic - for backward compatibility)
        public Report GenerateStockReport(string reportType)
        {
            return GenerateDailyStockReport(DateTime.Now);
        }

        public List<Product> GetLowStockProducts()
        {
            return new List<Product>
            {
                new Product { Name = "Wireless Mouse", Code = "MS002", Price = 800, Stock = 5, MinStock = 10, Unit = "PCS" },
                new Product { Name = "USB Cable", Code = "UC001", Price = 200, Stock = 2, MinStock = 20, Unit = "PCS" }
            };
        }

        // Sample data methods
        private List<StockMovement> GetSampleStockMovementData(DateTime date)
        {
            return new List<StockMovement>
            {
                new StockMovement { 
                    ItemName = "Laptop", 
                    ItemCode = "LP001",
                    OpeningStock = 25, 
                    Bought = 10, 
                    Sold = 8, 
                    ClosingStock = 27,
                    Unit = "PCS",
                    Date = date
                },
                new StockMovement { 
                    ItemName = "Wireless Mouse", 
                    ItemCode = "MS002",
                    OpeningStock = 45, 
                    Bought = 50, 
                    Sold = 35, 
                    ClosingStock = 60,
                    Unit = "PCS",
                    Date = date
                },
                new StockMovement { 
                    ItemName = "Keyboard", 
                    ItemCode = "KB001",
                    OpeningStock = 30, 
                    Bought = 25, 
                    Sold = 20, 
                    ClosingStock = 35,
                    Unit = "PCS",
                    Date = date
                }
            };
        }

        private List<StockMovement> GetSampleMonthlyStockData(int year, int month)
        {
            return new List<StockMovement>
            {
                new StockMovement { 
                    ItemName = "Laptop", 
                    ItemCode = "LP001",
                    OpeningStock = 20, 
                    Bought = 35, 
                    Sold = 28, 
                    ClosingStock = 27,
                    Unit = "PCS"
                },
                new StockMovement { 
                    ItemName = "Wireless Mouse", 
                    ItemCode = "MS002",
                    OpeningStock = 40, 
                    Bought = 150, 
                    Sold = 130, 
                    ClosingStock = 60,
                    Unit = "PCS"
                },
                new StockMovement { 
                    ItemName = "Keyboard", 
                    ItemCode = "KB001",
                    OpeningStock = 25, 
                    Bought = 80, 
                    Sold = 70, 
                    ClosingStock = 35,
                    Unit = "PCS"
                }
            };
        }

        private List<StockMovement> GetSampleYearlyStockData(int year)
        {
            return new List<StockMovement>
            {
                new StockMovement { 
                    ItemName = "Laptop", 
                    ItemCode = "LP001",
                    OpeningStock = 15, 
                    Bought = 150, 
                    Sold = 138, 
                    ClosingStock = 27,
                    Unit = "PCS"
                },
                new StockMovement { 
                    ItemName = "Wireless Mouse", 
                    ItemCode = "MS002",
                    OpeningStock = 30, 
                    Bought = 600, 
                    Sold = 570, 
                    ClosingStock = 60,
                    Unit = "PCS"
                },
                new StockMovement { 
                    ItemName = "Keyboard", 
                    ItemCode = "KB001",
                    OpeningStock = 20, 
                    Bought = 300, 
                    Sold = 285, 
                    ClosingStock = 35,
                    Unit = "PCS"
                }
            };
        }
    }

    // Stock Movement class for detailed tracking
    public class StockMovement
    {
        public string ItemName { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public decimal OpeningStock { get; set; }
        public decimal Bought { get; set; }
        public decimal Sold { get; set; }
        public decimal ClosingStock { get; set; }
        public string Unit { get; set; } = "PCS";
        public DateTime Date { get; set; } = DateTime.Now;

        public decimal ClosingStockCalculated => OpeningStock + Bought - Sold;
    }
}