using System;
using System.Data.SQLite;
using System.Windows.Forms;
using BillingSoftware.Models;
using System.Collections.Generic;
using System.Data;

namespace BillingSoftware.Modules
{
    public class DatabaseManager
    {
        private SQLiteConnection connection;
        private string connectionString = "Data Source=billing.db;Version=3;";

        public DatabaseManager()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                connection = new SQLiteConnection(connectionString);
                connection.Open();
                CreateTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CreateDatabase()
        {
            CreateTables();
        }

        private void CreateTables()
        {
            string[] tables = {
                // Vouchers table
                @"CREATE TABLE IF NOT EXISTS vouchers (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    type TEXT NOT NULL,
                    number TEXT UNIQUE NOT NULL,
                    date TEXT NOT NULL,
                    party TEXT,
                    amount DECIMAL(15,2) DEFAULT 0,
                    description TEXT,
                    status TEXT DEFAULT 'Active',
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP
                )",

                // Voucher Items table
                @"CREATE TABLE IF NOT EXISTS voucher_items (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    voucher_number TEXT NOT NULL,
                    product_name TEXT NOT NULL,
                    quantity DECIMAL(15,3) DEFAULT 0,
                    unit_price DECIMAL(15,2) DEFAULT 0,
                    total_amount DECIMAL(15,2) DEFAULT 0,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (voucher_number) REFERENCES vouchers (number)
                )",

                // Products table
                @"CREATE TABLE IF NOT EXISTS products (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    code TEXT UNIQUE NOT NULL,
                    price DECIMAL(15,2) DEFAULT 0,
                    stock DECIMAL(15,3) DEFAULT 0,
                    unit TEXT DEFAULT 'PCS',
                    min_stock DECIMAL(15,3) DEFAULT 10,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP
                )",

                // Estimate details table
                @"CREATE TABLE IF NOT EXISTS estimate_details (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    estimate_number TEXT NOT NULL,
                    discount_percent DECIMAL(5,2) DEFAULT 0,
                    tax_percent DECIMAL(5,2) DEFAULT 0,
                    expiry_date TEXT,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (estimate_number) REFERENCES vouchers (number)
                )",

                // Stock transactions table
                @"CREATE TABLE IF NOT EXISTS stock_transactions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    product_name TEXT NOT NULL,
                    transaction_type TEXT NOT NULL,
                    quantity DECIMAL(15,3) DEFAULT 0,
                    unit_price DECIMAL(15,2) DEFAULT 0,
                    total_amount DECIMAL(15,2) DEFAULT 0,
                    voucher_number TEXT,
                    transaction_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    notes TEXT,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP
                )"
            };

            foreach (string table in tables)
            {
                try
                {
                    using (var cmd = new SQLiteCommand(table, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating table: {ex.Message}");
                }
            }

            // Insert default data
            InsertDefaultData();
        }

        private void InsertDefaultData()
        {
            try
            {
                // Default admin user (if needed)
                string userSql = @"INSERT OR IGNORE INTO users (username, password, role) 
                                   VALUES ('admin', 'admin', 'Admin')";
                using (var cmd = new SQLiteCommand(userSql, connection))
                    cmd.ExecuteNonQuery();

                // Sample products
                string productsSql = @"INSERT OR IGNORE INTO products (name, code, price, stock, unit, min_stock) VALUES
                                      ('Laptop Dell XPS', 'LP001', 75000, 15, 'PCS', 5),
                                      ('Wireless Mouse', 'MS002', 1200, 30, 'PCS', 10),
                                      ('Keyboard', 'KB001', 3500, 20, 'PCS', 8),
                                      ('27-inch Monitor', 'MN001', 22000, 8, 'PCS', 3),
                                      ('USB-C Cable', 'UC001', 800, 50, 'PCS', 20)";
                using (var cmd = new SQLiteCommand(productsSql, connection))
                    cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting default data: {ex.Message}");
            }
        }

        public SQLiteConnection GetConnection()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }

        public void CloseConnection()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }

        // ============ PRODUCT METHODS ============
        public DataTable GetAllProducts()
        {
            var dataTable = new DataTable();
            try
            {
                string sql = "SELECT id, name, code, price, stock, unit, min_stock FROM products ORDER BY name";
                using (var cmd = new SQLiteCommand(sql, connection))
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        public Product GetProductByName(string productName)
        {
            try
            {
                string sql = "SELECT * FROM products WHERE name = @name";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", productName);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Code = reader["code"].ToString(),
                                Price = Convert.ToDecimal(reader["price"]),
                                Stock = Convert.ToDecimal(reader["stock"]),
                                Unit = reader["unit"].ToString(),
                                MinStock = Convert.ToDecimal(reader["min_stock"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting product: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            var products = new List<Product>();
            
            try
            {
                string sql = @"SELECT * FROM products 
                              WHERE name LIKE @searchTerm OR code LIKE @searchTerm
                              ORDER BY name";
                
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
                    
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching products: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return products;
        }

        public bool AddProduct(Product product)
        {
            try
            {
                string sql = @"INSERT INTO products (name, code, price, stock, unit, min_stock) 
                              VALUES (@name, @code, @price, @stock, @unit, @minStock)";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", product.Name);
                    cmd.Parameters.AddWithValue("@code", product.Code);
                    cmd.Parameters.AddWithValue("@price", product.Price);
                    cmd.Parameters.AddWithValue("@stock", product.Stock);
                    cmd.Parameters.AddWithValue("@unit", product.Unit);
                    cmd.Parameters.AddWithValue("@minStock", product.MinStock);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdateProductStock(string productName, decimal quantityChange)
        {
            try
            {
                string sql = @"UPDATE products SET stock = stock + @quantityChange 
                              WHERE name = @productName";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@quantityChange", quantityChange);
                    cmd.Parameters.AddWithValue("@productName", productName);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product stock: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ============ VOUCHER METHODS ============
        public bool AddVoucher(Voucher voucher)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Insert main voucher
                    string voucherSql = @"INSERT INTO vouchers (type, number, date, party, amount, description, status)
                                      VALUES (@type, @number, @date, @party, @amount, @description, @status)";

                    using (var cmd = new SQLiteCommand(voucherSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@type", voucher.Type);
                        cmd.Parameters.AddWithValue("@number", voucher.Number);
                        cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@party", voucher.Party);
                        cmd.Parameters.AddWithValue("@amount", voucher.Amount);
                        cmd.Parameters.AddWithValue("@description", voucher.Description);
                        cmd.Parameters.AddWithValue("@status", voucher.Status);

                        if (cmd.ExecuteNonQuery() == 0)
                            throw new Exception("Failed to insert voucher");
                    }

                    // Insert voucher items
                    foreach (var item in voucher.Items)
                    {
                        string itemSql = @"INSERT INTO voucher_items (voucher_number, product_name, quantity, unit_price, total_amount)
                                       VALUES (@voucherNumber, @productName, @quantity, @unitPrice, @totalAmount)";

                        using (var cmd = new SQLiteCommand(itemSql, connection, transaction))
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
                            UpdateProductStock(item.ProductName, -item.Quantity);
                            AddStockTransaction(item.ProductName, "SALE", item.Quantity, item.UnitPrice, voucher.Number);
                        }
                        else if (voucher.Type == "Stock Purchase")
                        {
                            UpdateProductStock(item.ProductName, item.Quantity);
                            AddStockTransaction(item.ProductName, "PURCHASE", item.Quantity, item.UnitPrice, voucher.Number);
                        }
                    }

                    // If estimate, save estimate details
                    if (voucher.Type == "Estimate")
                    {
                        // Parse discount and tax from description
                        decimal discount = 0;
                        decimal tax = 0;
                        
                        string estimateSql = @"INSERT INTO estimate_details (estimate_number, discount_percent, tax_percent, expiry_date)
                                           VALUES (@number, @discount, @tax, @expiry)";
                        
                        using (var cmd = new SQLiteCommand(estimateSql, connection, transaction))
                        {
                            // Extract from description (you might want to pass these as parameters)
                            cmd.Parameters.AddWithValue("@number", voucher.Number);
                            cmd.Parameters.AddWithValue("@discount", discount);
                            cmd.Parameters.AddWithValue("@tax", tax);
                            cmd.Parameters.AddWithValue("@expiry", DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Error adding voucher: {ex.Message}", "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        public DataTable GetVouchers()
        {
            var dataTable = new DataTable();
            try
            {
                string sql = @"SELECT number, type, date, party, amount, description 
                              FROM vouchers 
                              WHERE status = 'Active' 
                              ORDER BY date DESC";
                
                using (var cmd = new SQLiteCommand(sql, connection))
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vouchers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        public List<Voucher> GetSalesVouchers()
        {
            var vouchers = new List<Voucher>();
            
            try
            {
                string sql = @"SELECT v.*, 
                              GROUP_CONCAT(vi.product_name || '|' || vi.quantity || '|' || vi.unit_price, ';') as items
                              FROM vouchers v
                              LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                              WHERE v.type = 'Sales' AND v.status = 'Active'
                              GROUP BY v.id
                              ORDER BY v.date DESC";
                
                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vouchers.Add(ParseVoucherFromReader(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales vouchers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return vouchers;
        }

        public List<Voucher> GetPurchaseVouchers()
        {
            var vouchers = new List<Voucher>();
            
            try
            {
                string sql = @"SELECT v.*, 
                              GROUP_CONCAT(vi.product_name || '|' || vi.quantity || '|' || vi.unit_price, ';') as items
                              FROM vouchers v
                              LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                              WHERE v.type = 'Stock Purchase' AND v.status = 'Active'
                              GROUP BY v.id
                              ORDER BY v.date DESC";
                
                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vouchers.Add(ParseVoucherFromReader(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase vouchers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return vouchers;
        }

        private Voucher ParseVoucherFromReader(SQLiteDataReader reader)
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
                Status = reader["status"].ToString()
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

        public string GenerateVoucherNumber(string type)
        {
            string prefix = type switch
            {
                "Sales" => "SL",
                "Stock Purchase" => "STK",
                "Estimate" => "EST",
                _ => "VCH"
            };
            
            int count = GetVoucherCountByType(type) + 1;
            return $"{prefix}-{count.ToString().PadLeft(3, '0')}";
        }

        private int GetVoucherCountByType(string type)
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM vouchers WHERE type = @type";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@type", type);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // ============ STOCK TRANSACTION METHODS ============
        public bool AddStockTransaction(string productName, string transactionType, decimal quantity, 
                                      decimal unitPrice, string voucherNumber, string notes = "")
        {
            try
            {
                string sql = @"INSERT INTO stock_transactions 
                              (product_name, transaction_type, quantity, unit_price, total_amount, voucher_number, notes)
                              VALUES (@productName, @transactionType, @quantity, @unitPrice, @totalAmount, @voucherNumber, @notes)";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    cmd.Parameters.AddWithValue("@transactionType", transactionType);
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.Parameters.AddWithValue("@unitPrice", unitPrice);
                    cmd.Parameters.AddWithValue("@totalAmount", quantity * unitPrice);
                    cmd.Parameters.AddWithValue("@voucherNumber", voucherNumber);
                    cmd.Parameters.AddWithValue("@notes", notes);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding stock transaction: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public DataTable GetStockTransactions(DateTime fromDate, DateTime toDate)
        {
            var dataTable = new DataTable();
            try
            {
                string sql = @"SELECT st.transaction_date, st.product_name, st.transaction_type, 
                              st.quantity, st.unit_price, st.total_amount, st.voucher_number, st.notes
                              FROM stock_transactions st
                              WHERE DATE(st.transaction_date) BETWEEN @fromDate AND @toDate
                              ORDER BY st.transaction_date DESC";
                
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));
                    
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock transactions: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        // ============ REPORT METHODS ============
        public string GetDatabaseStats()
        {
            try
            {
                string stats = "";
                
                // Count products
                string productSql = "SELECT COUNT(*) as count FROM products";
                using (var cmd = new SQLiteCommand(productSql, connection))
                {
                    stats += $"📦 Products: {cmd.ExecuteScalar()}\n";
                }

                // Count vouchers by type
                string voucherSql = "SELECT type, COUNT(*) as count FROM vouchers WHERE status = 'Active' GROUP BY type";
                using (var cmd = new SQLiteCommand(voucherSql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stats += $"{reader["type"]}: {reader["count"]}\n";
                    }
                }

                // Total sales
                string salesSql = "SELECT SUM(amount) as total FROM vouchers WHERE type = 'Sales' AND status = 'Active'";
                using (var cmd = new SQLiteCommand(salesSql, connection))
                {
                    var result = cmd.ExecuteScalar();
                    decimal totalSales = result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                    stats += $"💰 Total Sales: ₹{totalSales:N2}\n";
                }

                // Low stock count
                string lowStockSql = "SELECT COUNT(*) FROM products WHERE stock <= min_stock";
                using (var cmd = new SQLiteCommand(lowStockSql, connection))
                {
                    int lowStockCount = Convert.ToInt32(cmd.ExecuteScalar());
                    if (lowStockCount > 0)
                        stats += $"⚠️ Low Stock Items: {lowStockCount}\n";
                }

                return stats;
            }
            catch (Exception ex)
            {
                return $"Error getting stats: {ex.Message}";
            }
        }

        // ============ UTILITY METHODS ============
        public bool BackupDatabase(string backupPath)
        {
            try
            {
                CloseConnection();
                System.IO.File.Copy("billing.db", backupPath, true);
                InitializeDatabase();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool RestoreDatabase(string backupPath)
        {
            try
            {
                var result = MessageBox.Show("This will replace all current data. Continue?", "Confirm Restore", 
                                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    CloseConnection();
                    System.IO.File.Copy(backupPath, "billing.db", true);
                    InitializeDatabase();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Restore failed: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void Dispose()
        {
            CloseConnection();
            connection?.Dispose();
        }
    }
}