using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
            string sql = "SELECT COUNT(*) FROM products WHERE stock <= min_stock AND stock > 0";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Daily Stock Report - REAL DATA
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

            // REAL DATABASE QUERY - Stock movements for the day
            string sql = @"SELECT p.name, p.code, p.stock, p.unit, p.price, 
                                  COALESCE(SUM(CASE WHEN st.transaction_type = 'PURCHASE' THEN st.quantity ELSE 0 END), 0) as purchased,
                                  COALESCE(SUM(CASE WHEN st.transaction_type = 'SALE' THEN st.quantity ELSE 0 END), 0) as sold
                           FROM products p
                           LEFT JOIN stock_transactions st ON p.name = st.product_name AND st.transaction_date = @date
                           GROUP BY p.name, p.code, p.stock, p.unit, p.price
                           ORDER BY p.name";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var stock = Convert.ToDecimal(reader["stock"]);
                        var purchased = Convert.ToDecimal(reader["purchased"]);
                        var sold = Convert.ToDecimal(reader["sold"]);
                        var openingStock = stock - purchased + sold;

                        var reportItem = new ReportItem
                        {
                            Description = reader["name"].ToString(),
                            Quantity = openingStock,
                            Amount = purchased,
                            Date = date,
                            Reference = $"Code: {reader["code"]} | Sold: {sold} | Closing: {stock} | Unit: {reader["unit"]}"
                        };

                        report.Items.Add(reportItem);
                        report.TotalAmount += stock * Convert.ToDecimal(reader["price"]);
                    }
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Monthly Stock Report - REAL DATA
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

            // REAL DATABASE QUERY - Monthly stock summary
            string sql = @"SELECT p.name, p.code, p.unit, p.price, p.stock as closing_stock,
                                  COALESCE(SUM(CASE WHEN st.transaction_type = 'PURCHASE' THEN st.quantity ELSE 0 END), 0) as monthly_purchased,
                                  COALESCE(SUM(CASE WHEN st.transaction_type = 'SALE' THEN st.quantity ELSE 0 END), 0) as monthly_sold
                           FROM products p
                           LEFT JOIN stock_transactions st ON p.name = st.product_name 
                           WHERE strftime('%Y-%m', st.transaction_date) = @yearMonth OR st.transaction_date IS NULL
                           GROUP BY p.name, p.code, p.unit, p.price, p.stock
                           ORDER BY p.name";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@yearMonth", $"{year}-{month:00}");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var monthlyPurchased = Convert.ToDecimal(reader["monthly_purchased"]);
                        var monthlySold = Convert.ToDecimal(reader["monthly_sold"]);
                        var closingStock = Convert.ToDecimal(reader["closing_stock"]);
                        var openingStock = closingStock - monthlyPurchased + monthlySold;

                        var reportItem = new ReportItem
                        {
                            Description = reader["name"].ToString(),
                            Quantity = openingStock,
                            Amount = monthlyPurchased,
                            Date = fromDate,
                            Reference = $"Purchased: {monthlyPurchased} | Sold: {monthlySold} | Closing: {closingStock}"
                        };

                        report.Items.Add(reportItem);
                        report.TotalAmount += closingStock * Convert.ToDecimal(reader["price"]);
                    }
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Yearly Stock Summary - REAL DATA
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

            // REAL DATABASE QUERY - Yearly stock summary
            string sql = @"SELECT p.name, p.code, p.unit, p.price, p.stock as closing_stock,
                                  COALESCE(SUM(CASE WHEN st.transaction_type = 'PURCHASE' THEN st.quantity ELSE 0 END), 0) as yearly_purchased,
                                  COALESCE(SUM(CASE WHEN st.transaction_type = 'SALE' THEN st.quantity ELSE 0 END), 0) as yearly_sold
                           FROM products p
                           LEFT JOIN stock_transactions st ON p.name = st.product_name 
                           WHERE strftime('%Y', st.transaction_date) = @year OR st.transaction_date IS NULL
                           GROUP BY p.name, p.code, p.unit, p.price, p.stock
                           ORDER BY p.name";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@year", year.ToString());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var yearlyPurchased = Convert.ToDecimal(reader["yearly_purchased"]);
                        var yearlySold = Convert.ToDecimal(reader["yearly_sold"]);
                        var closingStock = Convert.ToDecimal(reader["closing_stock"]);
                        var openingStock = closingStock - yearlyPurchased + yearlySold;

                        var reportItem = new ReportItem
                        {
                            Description = reader["name"].ToString(),
                            Quantity = openingStock,
                            Amount = yearlyPurchased,
                            Date = new DateTime(year, 1, 1),
                            Reference = $"Purchased: {yearlyPurchased} | Sold: {yearlySold} | Closing: {closingStock}"
                        };

                        report.Items.Add(reportItem);
                        report.TotalAmount += closingStock * Convert.ToDecimal(reader["price"]);
                    }
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Sales Report - REAL DATA with item details
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

            // REAL DATABASE QUERY with item details
            string sql = @"SELECT v.number, v.date, v.party, v.amount, v.description,
                                  GROUP_CONCAT(vi.product_name || ' (' || vi.quantity || ' x ' || vi.unit_price || ')', ', ') as items
                           FROM vouchers v
                           LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                           WHERE v.type = 'Sales' 
                           AND v.date BETWEEN @fromDate AND @toDate 
                           AND v.status = 'Active'
                           GROUP BY v.id
                           ORDER BY v.date DESC";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var itemsDescription = reader["items"].ToString();
                        var description = string.IsNullOrEmpty(itemsDescription) ? 
                            reader["description"].ToString() : 
                            $"Items: {itemsDescription}";

                        var item = new ReportItem
                        {
                            Description = $"{reader["party"]} - {description}",
                            Amount = Convert.ToDecimal(reader["amount"]),
                            Date = DateTime.Parse(reader["date"].ToString()),
                            Reference = reader["number"].ToString()
                        };

                        report.Items.Add(item);
                        report.TotalAmount += item.Amount;
                    }
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Financial Report - REAL DATA
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

            // REAL DATABASE QUERY - All financial transactions
            string sql = @"SELECT type, number, date, party, amount, description, reference_voucher
                           FROM vouchers 
                           WHERE date BETWEEN @fromDate AND @toDate 
                           AND status = 'Active'
                           ORDER BY date, type";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var reference = reader["reference_voucher"].ToString();
                        var referenceText = string.IsNullOrEmpty(reference) ? "" : $"(Ref: {reference})";

                        var item = new ReportItem
                        {
                            Description = $"{reader["type"]} - {reader["party"]} {referenceText}",
                            Amount = Convert.ToDecimal(reader["amount"]),
                            Date = DateTime.Parse(reader["date"].ToString()),
                            Reference = reader["number"].ToString()
                        };

                        report.Items.Add(item);
                        report.TotalAmount += item.Amount;
                    }
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Voucher Summary Report - REAL DATA
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

            // REAL DATABASE QUERY - Voucher summary by type
            string sql = @"SELECT type, COUNT(*) as count, SUM(amount) as total_amount
                           FROM vouchers 
                           WHERE date BETWEEN @fromDate AND @toDate 
                           AND status = 'Active'
                           GROUP BY type
                           ORDER BY type";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ReportItem
                        {
                            Description = $"{reader["type"]} Vouchers",
                            Quantity = Convert.ToInt32(reader["count"]),
                            Amount = Convert.ToDecimal(reader["total_amount"]),
                            Reference = $"{reader["count"]} transactions"
                        };

                        report.Items.Add(item);
                        report.TotalAmount += item.Amount;
                    }
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Stock Report - Shows ALL products including newly added ones
        public Report GenerateStockReport(string reportType)
        {
            var report = new Report
            {
                Type = "Stock",
                Title = $"Current Stock Report - {DateTime.Now:dd-MMM-yyyy}",
                GeneratedDate = DateTime.Now
            };

            // REAL DATABASE QUERY - ALL products (including zero stock)
            string sql = "SELECT * FROM products ORDER BY name";
            
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var stock = Convert.ToDecimal(reader["stock"]);
                    var price = Convert.ToDecimal(reader["price"]);
                    var minStock = Convert.ToDecimal(reader["min_stock"]);
                    
                    // Determine stock status
                    string stockStatus = "Normal";
                    if (stock == 0)
                        stockStatus = "Out of Stock";
                    else if (stock <= minStock)
                        stockStatus = "Low Stock";
                    
                    var item = new ReportItem
                    {
                        Description = reader["name"].ToString(),
                        Quantity = stock,
                        Amount = stock * price,
                        Reference = $"Code: {reader["code"]} | Min: {minStock} | Unit: {reader["unit"]} | Status: {stockStatus}"
                    };

                    report.Items.Add(item);
                    report.TotalAmount += item.Amount;
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Stock Valuation Report - REAL DATA
        public Report GenerateStockValuationReport()
        {
            var report = new Report
            {
                Type = "Stock Valuation",
                Title = $"Stock Valuation Report - {DateTime.Now:dd-MMM-yyyy}",
                GeneratedDate = DateTime.Now
            };

            // REAL DATABASE QUERY - Stock valuation
            string sql = @"SELECT name, code, stock, unit, price, (stock * price) as total_value
                           FROM products 
                           WHERE stock > 0
                           ORDER BY total_value DESC";
            
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = new ReportItem
                    {
                        Description = reader["name"].ToString(),
                        Quantity = Convert.ToDecimal(reader["stock"]),
                        Amount = Convert.ToDecimal(reader["total_value"]),
                        Reference = $"Code: {reader["code"]} | Price: {Convert.ToDecimal(reader["price"]):N2} | Unit: {reader["unit"]}"
                    };

                    report.Items.Add(item);
                    report.TotalAmount += item.Amount;
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        // Low Stock Products - REAL DATA
        public List<Product> GetLowStockProducts()
        {
            var lowStockProducts = new List<Product>();
            
            // REAL DATABASE QUERY - Low stock items
            string sql = "SELECT * FROM products WHERE stock <= min_stock ORDER BY stock ASC";
            
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lowStockProducts.Add(new Product
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        Code = reader["code"].ToString(),
                        Price = Convert.ToDecimal(reader["price"]),
                        Stock = Convert.ToDecimal(reader["stock"]),
                        MinStock = Convert.ToDecimal(reader["min_stock"]),
                        Unit = reader["unit"].ToString()
                    });
                }
            }
            
            return lowStockProducts;
        }

        // Get Stock Movement History
        public List<StockMovement> GetStockMovementHistory(string productName, DateTime fromDate, DateTime toDate)
        {
            var movements = new List<StockMovement>();
            
            string sql = @"SELECT * FROM stock_transactions 
                           WHERE product_name = @productName 
                           AND transaction_date BETWEEN @fromDate AND @toDate
                           ORDER BY transaction_date DESC";
            
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@productName", productName);
                cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        movements.Add(new StockMovement
                        {
                            ItemName = reader["product_name"].ToString(),
                            TransactionType = reader["transaction_type"].ToString(),
                            Quantity = Convert.ToDecimal(reader["quantity"]),
                            UnitPrice = Convert.ToDecimal(reader["unit_price"]),
                            VoucherNumber = reader["voucher_number"].ToString(),
                            Date = DateTime.Parse(reader["transaction_date"].ToString()),
                            Notes = reader["notes"].ToString()
                        });
                    }
                }
            }
            
            return movements;
        }
    }

    // Stock Movement class for detailed tracking
    public class StockMovement
    {
        public string ItemName { get; set; } = "";
        public string TransactionType { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount => Quantity * UnitPrice;
        public string VoucherNumber { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public string Notes { get; set; } = "";
    }
}