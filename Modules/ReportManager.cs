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

        // Daily Stock Report - Simple version without price
        public List<SimpleStockReportItem> GenerateDailyStockReport(DateTime date)
        {
            var stockItems = new List<SimpleStockReportItem>();

            string sql = @"SELECT 
                    p.name as Item,
                    p.code as Code,
                    p.unit as Unit,
                    p.stock as CurrentStock,
                    -- Opening Balance (Stock before today)
                    (p.stock - 
                        COALESCE((
                            SELECT SUM(CASE 
                                WHEN v.type = 'Stock Purchase' THEN vi.quantity
                                WHEN v.type = 'Sales' THEN -vi.quantity
                                ELSE 0 
                            END)
                            FROM voucher_items vi 
                            JOIN vouchers v ON vi.voucher_number = v.number
                            WHERE vi.product_name = p.name 
                            AND DATE(v.date) = @date
                        ), 0)
                    ) as OpeningBalance,
                    -- Today's Purchases
                    COALESCE((
                        SELECT SUM(vi.quantity) 
                        FROM voucher_items vi 
                        JOIN vouchers v ON vi.voucher_number = v.number
                        WHERE vi.product_name = p.name 
                        AND v.type = 'Stock Purchase'
                        AND DATE(v.date) = @date
                    ), 0) as Purchased,
                    -- Today's Sales
                    COALESCE((
                        SELECT SUM(vi.quantity) 
                        FROM voucher_items vi 
                        JOIN vouchers v ON vi.voucher_number = v.number
                        WHERE vi.product_name = p.name 
                        AND v.type = 'Sales'
                        AND DATE(v.date) = @date
                    ), 0) as Sold
                   FROM products p
                   ORDER BY p.name";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stockItems.Add(new SimpleStockReportItem
                        {
                            Item = reader["Item"].ToString(),
                            Code = reader["Code"].ToString(),
                            Unit = reader["Unit"].ToString(),
                            OpeningBalance = Convert.ToDecimal(reader["OpeningBalance"]),
                            Purchased = Convert.ToDecimal(reader["Purchased"]),
                            Sold = Convert.ToDecimal(reader["Sold"]),
                            ClosingBalance = Convert.ToDecimal(reader["CurrentStock"])
                        });
                    }
                }
            }

            return stockItems;
        }

        // Monthly Stock Report - Simple version without price
        public List<SimpleStockReportItem> GenerateMonthlyStockReport(int year, int month)
        {
            var stockItems = new List<SimpleStockReportItem>();
            var fromDate = new DateTime(year, month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);

            string sql = @"SELECT 
                    p.name as Item,
                    p.code as Code,
                    p.unit as Unit,
                    p.stock as CurrentStock,
                    -- Opening Balance (Stock before this month)
                    (p.stock - 
                        COALESCE((
                            SELECT SUM(CASE 
                                WHEN v.type = 'Stock Purchase' THEN vi.quantity
                                WHEN v.type = 'Sales' THEN -vi.quantity
                                ELSE 0 
                            END)
                            FROM voucher_items vi 
                            JOIN vouchers v ON vi.voucher_number = v.number
                            WHERE vi.product_name = p.name 
                            AND DATE(v.date) BETWEEN @fromDate AND @toDate
                        ), 0)
                    ) as OpeningBalance,
                    -- Monthly Purchases
                    COALESCE((
                        SELECT SUM(vi.quantity) 
                        FROM voucher_items vi 
                        JOIN vouchers v ON vi.voucher_number = v.number
                        WHERE vi.product_name = p.name 
                        AND v.type = 'Stock Purchase'
                        AND DATE(v.date) BETWEEN @fromDate AND @toDate
                    ), 0) as Purchased,
                    -- Monthly Sales
                    COALESCE((
                        SELECT SUM(vi.quantity) 
                        FROM voucher_items vi 
                        JOIN vouchers v ON vi.voucher_number = v.number
                        WHERE vi.product_name = p.name 
                        AND v.type = 'Sales'
                        AND DATE(v.date) BETWEEN @fromDate AND @toDate
                    ), 0) as Sold
                   FROM products p
                   ORDER BY p.name";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stockItems.Add(new SimpleStockReportItem
                        {
                            Item = reader["Item"].ToString(),
                            Code = reader["Code"].ToString(),
                            Unit = reader["Unit"].ToString(),
                            OpeningBalance = Convert.ToDecimal(reader["OpeningBalance"]),
                            Purchased = Convert.ToDecimal(reader["Purchased"]),
                            Sold = Convert.ToDecimal(reader["Sold"]),
                            ClosingBalance = Convert.ToDecimal(reader["CurrentStock"])
                        });
                    }
                }
            }

            return stockItems;
        }

        // Yearly Stock Report - Simple version without price
        public List<SimpleStockReportItem> GenerateYearlyStockReport(int year)
        {
            var stockItems = new List<SimpleStockReportItem>();
            var fromDate = new DateTime(year, 1, 1);
            var toDate = new DateTime(year, 12, 31);

            string sql = @"SELECT 
                    p.name as Item,
                    p.code as Code,
                    p.unit as Unit,
                    p.stock as CurrentStock,
                    -- Opening Balance (Stock before this year)
                    (p.stock - 
                        COALESCE((
                            SELECT SUM(CASE 
                                WHEN v.type = 'Stock Purchase' THEN vi.quantity
                                WHEN v.type = 'Sales' THEN -vi.quantity
                                ELSE 0 
                            END)
                            FROM voucher_items vi 
                            JOIN vouchers v ON vi.voucher_number = v.number
                            WHERE vi.product_name = p.name 
                            AND strftime('%Y', v.date) = @year
                        ), 0)
                    ) as OpeningBalance,
                    -- Yearly Purchases
                    COALESCE((
                        SELECT SUM(vi.quantity) 
                        FROM voucher_items vi 
                        JOIN vouchers v ON vi.voucher_number = v.number
                        WHERE vi.product_name = p.name 
                        AND v.type = 'Stock Purchase'
                        AND strftime('%Y', v.date) = @year
                    ), 0) as Purchased,
                    -- Yearly Sales
                    COALESCE((
                        SELECT SUM(vi.quantity) 
                        FROM voucher_items vi 
                        JOIN vouchers v ON vi.voucher_number = v.number
                        WHERE vi.product_name = p.name 
                        AND v.type = 'Sales'
                        AND strftime('%Y', v.date) = @year
                    ), 0) as Sold
                   FROM products p
                   ORDER BY p.name";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@year", year.ToString());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stockItems.Add(new SimpleStockReportItem
                        {
                            Item = reader["Item"].ToString(),
                            Code = reader["Code"].ToString(),
                            Unit = reader["Unit"].ToString(),
                            OpeningBalance = Convert.ToDecimal(reader["OpeningBalance"]),
                            Purchased = Convert.ToDecimal(reader["Purchased"]),
                            Sold = Convert.ToDecimal(reader["Sold"]),
                            ClosingBalance = Convert.ToDecimal(reader["CurrentStock"])
                        });
                    }
                }
            }

            return stockItems;
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