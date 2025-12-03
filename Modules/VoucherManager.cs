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
        private LedgerManager ledgerManager;

        public VoucherManager()
        {
            dbManager = new DatabaseManager();
            ledgerManager = new LedgerManager();
        }

        // Voucher Operations with items
        public bool AddVoucher(Voucher voucher)
        {
            using (var transaction = dbManager.GetConnection().BeginTransaction())
            {
                try
                {
                    // Insert main voucher
                    string voucherSql = @"INSERT INTO vouchers (type, number, date, party, amount, description, status, created_by)
                                      VALUES (@type, @number, @date, @party, @amount, @description, @status, @createdBy)";

                    using (var cmd = new SQLiteCommand(voucherSql, dbManager.GetConnection(), transaction))
                    {
                        cmd.Parameters.AddWithValue("@type", voucher.Type);
                        cmd.Parameters.AddWithValue("@number", voucher.Number);
                        cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@party", voucher.Party);
                        cmd.Parameters.AddWithValue("@amount", voucher.Amount);
                        cmd.Parameters.AddWithValue("@description", voucher.Description);
                        cmd.Parameters.AddWithValue("@status", voucher.Status);
                        cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);

                        if (cmd.ExecuteNonQuery() == 0)
                            throw new Exception("Failed to insert voucher");
                    }

                    // Insert voucher items
                    foreach (var item in voucher.Items)
                    {
                        string itemSql = @"INSERT INTO voucher_items (voucher_number, product_name, quantity, unit_price, total_amount)
                                       VALUES (@voucherNumber, @productName, @quantity, @unitPrice, @totalAmount)";

                        using (var cmd = new SQLiteCommand(itemSql, dbManager.GetConnection(), transaction))
                        {
                            cmd.Parameters.AddWithValue("@voucherNumber", voucher.Number);
                            cmd.Parameters.AddWithValue("@productName", item.ProductName);
                            cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                            cmd.Parameters.AddWithValue("@totalAmount", item.TotalAmount);

                            if (cmd.ExecuteNonQuery() == 0)
                                throw new Exception("Failed to insert voucher item");
                        }

                        // Update stock for sales and purchases
                        if (voucher.Type == "Sales")
                        {
                            UpdateProductStock(item.ProductName, -item.Quantity, voucher.Number, "SALE");
                        }
                        else if (voucher.Type == "Stock Purchase")
                        {
                            UpdateProductStock(item.ProductName, item.Quantity, voucher.Number, "PURCHASE");
                        }
                    }

                    // Post to ledger based on voucher type
                    switch (voucher.Type)
                    {
                        case "Sales":
                            if (!ledgerManager.PostSalesTransaction(voucher))
                                throw new Exception("Failed to post sales to ledger");
                            break;
                            
                        case "Stock Purchase":
                            if (!ledgerManager.PostPurchaseTransaction(voucher))
                                throw new Exception("Failed to post purchase to ledger");
                            break;
                            
                        case "Estimate":
                            // Estimates don't post to ledger
                            break;
                    }

                    transaction.Commit();
                    
                    // Log audit
                    AuditLogger auditLogger = new AuditLogger();
                    auditLogger.LogAction("CREATE", "VOUCHER", voucher.Number, 
                                        $"Created {voucher.Type} voucher: {voucher.Number} - Amount: ₹{voucher.Amount:N2}", 
                                        voucher.Type, voucher.Party);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error adding voucher: {ex.Message}");
                    return false;
                }
            }
        }

        public List<Voucher> GetAllVouchers()
        {
            var vouchers = new List<Voucher>();

            string sql = @"SELECT v.*, 
                          GROUP_CONCAT(vi.product_name || '|' || vi.quantity || '|' || vi.unit_price, ';') as items
                          FROM vouchers v
                          LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                          GROUP BY v.id
                          ORDER BY v.date DESC";

            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var voucher = new Voucher
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Type = reader["type"].ToString(),
                        Number = reader["number"].ToString(),
                        Date = DateTime.Parse(reader["date"].ToString()),
                        Party = reader["party"].ToString(),
                        Amount = Convert.ToDecimal(reader["amount"]),
                        Description = reader["description"].ToString(),
                        Status = reader["status"].ToString(),
                        CreatedBy = reader["created_by"].ToString()
                    };

                    // Parse items
                    var itemsData = reader["items"].ToString();
                    if (!string.IsNullOrEmpty(itemsData))
                    {
                        var items = itemsData.Split(';');
                        foreach (var item in items)
                        {
                            var parts = item.Split('|');
                            if (parts.Length >= 3)
                            {
                                voucher.Items.Add(new VoucherItem
                                {
                                    ProductName = parts[0],
                                    Quantity = decimal.Parse(parts[1]),
                                    UnitPrice = decimal.Parse(parts[2])
                                });
                            }
                        }
                    }

                    vouchers.Add(voucher);
                }
            }

            return vouchers;
        }

        public List<Voucher> GetVouchersByType(string type)
        {
            return GetAllVouchers().Where(v => v.Type == type).ToList();
        }

        public List<Voucher> GetSalesVouchersForReference()
        {
            string sql = @"SELECT v.*, 
                          GROUP_CONCAT(vi.product_name || '|' || vi.quantity || '|' || vi.unit_price, ';') as items
                          FROM vouchers v
                          LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                          WHERE v.type = 'Sales' AND v.status = 'Active'
                          GROUP BY v.id
                          ORDER BY v.date DESC";
            
            var vouchers = new List<Voucher>();
            
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var voucher = new Voucher
                    {
                        Number = reader["number"].ToString(),
                        Date = DateTime.Parse(reader["date"].ToString()),
                        Party = reader["party"].ToString(),
                        Amount = Convert.ToDecimal(reader["amount"]),
                        Description = reader["description"].ToString()
                    };
                    
                    // Parse items
                    var itemsData = reader["items"].ToString();
                    if (!string.IsNullOrEmpty(itemsData))
                    {
                        var items = itemsData.Split(';');
                        foreach (var item in items)
                        {
                            var parts = item.Split('|');
                            if (parts.Length >= 3)
                            {
                                voucher.Items.Add(new VoucherItem
                                {
                                    ProductName = parts[0],
                                    Quantity = decimal.Parse(parts[1]),
                                    UnitPrice = decimal.Parse(parts[2])
                                });
                            }
                        }
                    }
                    
                    vouchers.Add(voucher);
                }
            }
            return vouchers;
        }

        public List<Voucher> GetPurchaseVouchersForReference()
        {
            string sql = @"SELECT v.*, 
                          GROUP_CONCAT(vi.product_name || '|' || vi.quantity || '|' || vi.unit_price, ';') as items
                          FROM vouchers v
                          LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                          WHERE v.type = 'Stock Purchase' AND v.status = 'Active'
                          GROUP BY v.id
                          ORDER BY v.date DESC";
            
            var vouchers = new List<Voucher>();
            
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var voucher = new Voucher
                    {
                        Number = reader["number"].ToString(),
                        Date = DateTime.Parse(reader["date"].ToString()),
                        Party = reader["party"].ToString(),
                        Amount = Convert.ToDecimal(reader["amount"]),
                        Description = reader["description"].ToString()
                    };
                    
                    // Parse items
                    var itemsData = reader["items"].ToString();
                    if (!string.IsNullOrEmpty(itemsData))
                    {
                        var items = itemsData.Split(';');
                        foreach (var item in items)
                        {
                            var parts = item.Split('|');
                            if (parts.Length >= 3)
                            {
                                voucher.Items.Add(new VoucherItem
                                {
                                    ProductName = parts[0],
                                    Quantity = decimal.Parse(parts[1]),
                                    UnitPrice = decimal.Parse(parts[2])
                                });
                            }
                        }
                    }
                    
                    vouchers.Add(voucher);
                }
            }
            return vouchers;
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
            return $"{prefix}-{DateTime.Now:yyyyMMdd}-{count.ToString("000")}";
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
                "Stock Purchase" => "STK",
                _ => "VCH"
            };
        }

        private int GetVoucherCountByType(string type)
        {
            string sql = "SELECT COUNT(*) FROM vouchers WHERE type = @type AND strftime('%Y-%m-%d', date) = date('now')";
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@type", type);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void UpdateProductStock(string productName, decimal quantityChange, string voucherNumber, string transactionType)
        {
            try
            {
                // Update product stock
                string updateSql = @"UPDATE products SET stock = stock + @quantityChange 
                                  WHERE name = @productName";

                using (var cmd = new SQLiteCommand(updateSql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@quantityChange", quantityChange);
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.ExecuteNonQuery();
                }

                // Record stock transaction
                string transactionSql = @"INSERT INTO stock_transactions 
                                        (product_name, transaction_type, quantity, voucher_number, notes)
                                        VALUES (@productName, @transactionType, @quantity, @voucherNumber, @notes)";

                using (var cmd = new SQLiteCommand(transactionSql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@transactionType", transactionType);
                    cmd.Parameters.AddWithValue("@quantity", Math.Abs(quantityChange));
                    cmd.Parameters.AddWithValue("@voucherNumber", voucherNumber);
                    cmd.Parameters.AddWithValue("@notes", $"{transactionType} via {voucherNumber}");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product stock: {ex.Message}");
                throw;
            }
        }
        
        // Get voucher by number with all details
        public Voucher GetVoucherByNumber(string voucherNumber)
        {
            try
            {
                string sql = @"SELECT v.*, 
                              GROUP_CONCAT(vi.product_name || '|' || vi.quantity || '|' || vi.unit_price, ';') as items
                              FROM vouchers v
                              LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                              WHERE v.number = @number
                              GROUP BY v.id";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@number", voucherNumber);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var voucher = new Voucher
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Type = reader["type"].ToString(),
                                Number = reader["number"].ToString(),
                                Date = DateTime.Parse(reader["date"].ToString()),
                                Party = reader["party"].ToString(),
                                Amount = Convert.ToDecimal(reader["amount"]),
                                Description = reader["description"].ToString(),
                                Status = reader["status"].ToString(),
                                CreatedBy = reader["created_by"].ToString()
                            };
                            
                            // Parse items
                            var itemsData = reader["items"].ToString();
                            if (!string.IsNullOrEmpty(itemsData))
                            {
                                var items = itemsData.Split(';');
                                foreach (var item in items)
                                {
                                    var parts = item.Split('|');
                                    if (parts.Length >= 3)
                                    {
                                        voucher.Items.Add(new VoucherItem
                                        {
                                            ProductName = parts[0],
                                            Quantity = decimal.Parse(parts[1]),
                                            UnitPrice = decimal.Parse(parts[2])
                                        });
                                    }
                                }
                            }
                            
                            return voucher;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting voucher: {ex.Message}");
            }
            
            return null;
        }
    }
}