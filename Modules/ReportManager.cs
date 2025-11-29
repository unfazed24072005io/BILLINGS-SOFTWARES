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

        public int GetLowStockCount()
        {
            string sql = "SELECT COUNT(*) FROM products WHERE stock <= min_stock";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public Report GenerateStockReport(string reportType)
        {
            var report = new Report
            {
                Type = "Stock",
                Title = $"{reportType} Stock Report",
                FromDate = DateTime.Now.AddDays(-30),
                ToDate = DateTime.Now
            };

            string sql = "SELECT * FROM products ORDER BY name";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = new ReportItem
                    {
                        Description = reader["name"].ToString(),
                        Quantity = Convert.ToDecimal(reader["stock"]),
                        Amount = Convert.ToDecimal(reader["price"]),
                        Reference = reader["code"].ToString()
                    };

                    report.Items.Add(item);
                    report.TotalAmount += item.Amount * item.Quantity;
                }
            }

            report.TotalRecords = report.Items.Count;
            return report;
        }

        public Report GenerateSalesReport(DateTime fromDate, DateTime toDate)
        {
            var report = new Report
            {
                Type = "Sales",
                Title = $"Sales Report ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy})",
                FromDate = fromDate,
                ToDate = toDate
            };

            string sql = @"SELECT * FROM vouchers 
                          WHERE type = 'Sales' AND date BETWEEN @fromDate AND @toDate 
                          ORDER BY date";

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
                            Description = reader["description"].ToString(),
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

        public List<Product> GetLowStockProducts()
        {
            var products = new List<Product>();

            string sql = "SELECT * FROM products WHERE stock <= min_stock ORDER BY stock ASC";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        Code = reader["code"].ToString(),
                        Price = Convert.ToDecimal(reader["price"]),
                        Stock = Convert.ToDecimal(reader["stock"]),
                        Unit = reader["unit"].ToString(),
                        MinStock = Convert.ToDecimal(reader["min_stock"])
                    });
                }
            }

            return products;
        }
    }
}