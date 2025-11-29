using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using BillingSoftware.Models;

namespace BillingSoftware.Modules
{
    public class VoucherManager
    {
        private DatabaseManager dbManager;

        public VoucherManager()
        {
            dbManager = new DatabaseManager();
        }

        // Voucher Operations
        public bool AddVoucher(Voucher voucher)
        {
            try
            {
                string sql = @"INSERT INTO vouchers (type, number, date, party, amount, description, status)
                              VALUES (@type, @number, @date, @party, @amount, @description, @status)";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@type", voucher.Type);
                    cmd.Parameters.AddWithValue("@number", voucher.Number);
                    cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@party", voucher.Party);
                    cmd.Parameters.AddWithValue("@amount", voucher.Amount);
                    cmd.Parameters.AddWithValue("@description", voucher.Description);
                    cmd.Parameters.AddWithValue("@status", voucher.Status);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding voucher: {ex.Message}");
                return false;
            }
        }

        public List<Voucher> GetAllVouchers()
        {
            var vouchers = new List<Voucher>();

            string sql = "SELECT * FROM vouchers ORDER BY date DESC";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    vouchers.Add(new Voucher
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Type = reader["type"].ToString(),
                        Number = reader["number"].ToString(),
                        Date = DateTime.Parse(reader["date"].ToString()),
                        Party = reader["party"].ToString(),
                        Amount = Convert.ToDecimal(reader["amount"]),
                        Description = reader["description"].ToString(),
                        Status = reader["status"].ToString()
                    });
                }
            }

            return vouchers;
        }

        public List<Voucher> GetVouchersByType(string type)
        {
            return GetAllVouchers().Where(v => v.Type == type).ToList();
        }

        public int GetVouchersCount()
        {
            string sql = "SELECT COUNT(*) FROM vouchers WHERE status = 'Active'";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public decimal GetTotalSales()
        {
            string sql = "SELECT SUM(amount) FROM vouchers WHERE type = 'Sales' AND status = 'Active'";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
            }
        }

        public string GenerateVoucherNumber(string type)
        {
            string prefix = GetVoucherPrefix(type);
            int count = GetVoucherCountByType(type) + 1;
            return $"{prefix}-{count.ToString().PadLeft(3, '0')}";
        }

        private string GetVoucherPrefix(string type)
        {
            return type switch
            {
                "Sales" => "SL",
                "Receipt" => "RCPT",
                "Payment" => "PAY",
                "Journal" => "JRN",
                "Estimate" => "EST",
                _ => "VCH"
            };
        }

        private int GetVoucherCountByType(string type)
        {
            string sql = "SELECT COUNT(*) FROM vouchers WHERE type = @type";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@type", type);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}