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

        public ReportManager()
        {
            dbManager = new DatabaseManager();
        }

        // Daily Stock Report - CORRECTED calculation
        public List<SimpleStockReportItem> GenerateDailyStockReport(DateTime date)
        {
            var stockItems = new List<SimpleStockReportItem>();

            // Get all products
            string productsSql = "SELECT name, code, unit FROM products ORDER BY name";
            var products = new List<(string Name, string Code, string Unit)>();
            
            using (var cmd = new SQLiteCommand(productsSql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add((reader["name"].ToString(), 
                                 reader["code"].ToString(), 
                                 reader["unit"].ToString()));
                }
            }

            foreach (var product in products)
            {
                // Calculate opening balance (stock at START of day)
                decimal openingBalance = CalculateOpeningBalance(product.Name, date);
                
                // Calculate purchases for the day
                decimal purchased = CalculateDailyPurchases(product.Name, date);
                
                // Calculate sales for the day
                decimal sold = CalculateDailySales(product.Name, date);
                
                // Closing balance = Opening + Purchases - Sales
                decimal closingBalance = openingBalance + purchased - sold;
                
                // Get current stock for reference (not used in calculation)
                decimal currentStock = GetCurrentStock(product.Name);
                
                stockItems.Add(new SimpleStockReportItem
                {
                    Item = product.Name,
                    Code = product.Code,
                    Unit = product.Unit,
                    OpeningBalance = openingBalance,
                    Purchased = purchased,
                    Sold = sold,
                    ClosingBalance = closingBalance
                });
            }

            return stockItems;
        }

        // Calculate opening balance (stock at START of day)
        private decimal CalculateOpeningBalance(string productName, DateTime date)
        {
            try
            {
                // Opening balance = stock before this date
                // = Stock from all purchases before date - all sales before date
                string sql = @"
                    SELECT 
                        COALESCE((
                            SELECT SUM(vi.quantity) 
                            FROM voucher_items vi 
                            JOIN vouchers v ON vi.voucher_number = v.number
                            WHERE vi.product_name = @productName 
                            AND v.type = 'Stock Purchase'
                            AND DATE(v.date) < @date
                        ), 0) -
                        COALESCE((
                            SELECT SUM(vi.quantity) 
                            FROM voucher_items vi 
                            JOIN vouchers v ON vi.voucher_number = v.number
                            WHERE vi.product_name = @productName 
                            AND v.type = 'Sales'
                            AND DATE(v.date) < @date
                        ), 0) as OpeningBalance";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    
                    object result = cmd.ExecuteScalar();
                    decimal openingBalance = result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                    
                    // Opening balance cannot be negative
                    return openingBalance < 0 ? 0 : openingBalance;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Calculate purchases for a specific day
        private decimal CalculateDailyPurchases(string productName, DateTime date)
        {
            try
            {
                string sql = @"
                    SELECT COALESCE(SUM(vi.quantity), 0) 
                    FROM voucher_items vi 
                    JOIN vouchers v ON vi.voucher_number = v.number
                    WHERE vi.product_name = @productName 
                    AND v.type = 'Stock Purchase'
                    AND DATE(v.date) = @date";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Calculate sales for a specific day
        private decimal CalculateDailySales(string productName, DateTime date)
        {
            try
            {
                string sql = @"
                    SELECT COALESCE(SUM(vi.quantity), 0) 
                    FROM voucher_items vi 
                    JOIN vouchers v ON vi.voucher_number = v.number
                    WHERE vi.product_name = @productName 
                    AND v.type = 'Sales'
                    AND DATE(v.date) = @date";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Get current stock from products table
        private decimal GetCurrentStock(string productName)
        {
            try
            {
                string sql = "SELECT stock FROM products WHERE name = @productName";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Monthly Stock Report - CORRECTED calculation
        public List<SimpleStockReportItem> GenerateMonthlyStockReport(int year, int month)
        {
            var stockItems = new List<SimpleStockReportItem>();
            var fromDate = new DateTime(year, month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);

            // Get all products
            string productsSql = "SELECT name, code, unit FROM products ORDER BY name";
            var products = new List<(string Name, string Code, string Unit)>();
            
            using (var cmd = new SQLiteCommand(productsSql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add((reader["name"].ToString(), 
                                 reader["code"].ToString(), 
                                 reader["unit"].ToString()));
                }
            }

            foreach (var product in products)
            {
                // Calculate opening balance (stock at START of month)
                decimal openingBalance = CalculateOpeningBalance(product.Name, fromDate);
                
                // Calculate purchases for the month
                decimal purchased = CalculateMonthlyPurchases(product.Name, year, month);
                
                // Calculate sales for the month
                decimal sold = CalculateMonthlySales(product.Name, year, month);
                
                // Closing balance = Opening + Purchases - Sales
                decimal closingBalance = openingBalance + purchased - sold;
                
                stockItems.Add(new SimpleStockReportItem
                {
                    Item = product.Name,
                    Code = product.Code,
                    Unit = product.Unit,
                    OpeningBalance = openingBalance,
                    Purchased = purchased,
                    Sold = sold,
                    ClosingBalance = closingBalance
                });
            }

            return stockItems;
        }

        private decimal CalculateMonthlyPurchases(string productName, int year, int month)
        {
            try
            {
                string sql = @"
                    SELECT COALESCE(SUM(vi.quantity), 0) 
                    FROM voucher_items vi 
                    JOIN vouchers v ON vi.voucher_number = v.number
                    WHERE vi.product_name = @productName 
                    AND v.type = 'Stock Purchase'
                    AND strftime('%Y', v.date) = @year
                    AND strftime('%m', v.date) = @month";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@year", year.ToString());
                    cmd.Parameters.AddWithValue("@month", month.ToString("00"));
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private decimal CalculateMonthlySales(string productName, int year, int month)
        {
            try
            {
                string sql = @"
                    SELECT COALESCE(SUM(vi.quantity), 0) 
                    FROM voucher_items vi 
                    JOIN vouchers v ON vi.voucher_number = v.number
                    WHERE vi.product_name = @productName 
                    AND v.type = 'Sales'
                    AND strftime('%Y', v.date) = @year
                    AND strftime('%m', v.date) = @month";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@year", year.ToString());
                    cmd.Parameters.AddWithValue("@month", month.ToString("00"));
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Yearly Stock Report - CORRECTED calculation
        public List<SimpleStockReportItem> GenerateYearlyStockReport(int year)
        {
            var stockItems = new List<SimpleStockReportItem>();
            var fromDate = new DateTime(year, 1, 1);
            var toDate = new DateTime(year, 12, 31);

            // Get all products
            string productsSql = "SELECT name, code, unit FROM products ORDER BY name";
            var products = new List<(string Name, string Code, string Unit)>();
            
            using (var cmd = new SQLiteCommand(productsSql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add((reader["name"].ToString(), 
                                 reader["code"].ToString(), 
                                 reader["unit"].ToString()));
                }
            }

            foreach (var product in products)
            {
                // Calculate opening balance (stock at START of year)
                decimal openingBalance = CalculateOpeningBalance(product.Name, fromDate);
                
                // Calculate purchases for the year
                decimal purchased = CalculateYearlyPurchases(product.Name, year);
                
                // Calculate sales for the year
                decimal sold = CalculateYearlySales(product.Name, year);
                
                // Closing balance = Opening + Purchases - Sales
                decimal closingBalance = openingBalance + purchased - sold;
                
                stockItems.Add(new SimpleStockReportItem
                {
                    Item = product.Name,
                    Code = product.Code,
                    Unit = product.Unit,
                    OpeningBalance = openingBalance,
                    Purchased = purchased,
                    Sold = sold,
                    ClosingBalance = closingBalance
                });
            }

            return stockItems;
        }

        private decimal CalculateYearlyPurchases(string productName, int year)
        {
            try
            {
                string sql = @"
                    SELECT COALESCE(SUM(vi.quantity), 0) 
                    FROM voucher_items vi 
                    JOIN vouchers v ON vi.voucher_number = v.number
                    WHERE vi.product_name = @productName 
                    AND v.type = 'Stock Purchase'
                    AND strftime('%Y', v.date) = @year";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@year", year.ToString());
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private decimal CalculateYearlySales(string productName, int year)
        {
            try
            {
                string sql = @"
                    SELECT COALESCE(SUM(vi.quantity), 0) 
                    FROM voucher_items vi 
                    JOIN vouchers v ON vi.voucher_number = v.number
                    WHERE vi.product_name = @productName 
                    AND v.type = 'Sales'
                    AND strftime('%Y', v.date) = @year";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@year", year.ToString());
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    // Simple Stock Report Item class (NO PRICE FIELD)
    public class SimpleStockReportItem
    {
        public string Item { get; set; } = "";
        public string Code { get; set; } = "";
        public string Unit { get; set; } = "PCS";
        public decimal OpeningBalance { get; set; }
        public decimal Purchased { get; set; }
        public decimal Sold { get; set; }
        public decimal ClosingBalance { get; set; }
    }
}