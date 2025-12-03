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
                // Users table
                @"CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    role TEXT DEFAULT 'User',
                    full_name TEXT,
                    email TEXT,
                    is_active INTEGER DEFAULT 1,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP
                )",

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
                    reference_voucher TEXT,
		    created_by TEXT,
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
                )",

                // NEW: Ledger tables
                @"CREATE TABLE IF NOT EXISTS ledgers (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    code TEXT UNIQUE NOT NULL,
                    type TEXT NOT NULL,
                    contact_person TEXT,
                    phone TEXT,
                    email TEXT,
                    address TEXT,
                    gstin TEXT,
                    opening_balance DECIMAL(15,2) DEFAULT 0,
                    balance_type TEXT DEFAULT 'Dr',
                    credit_limit DECIMAL(15,2) DEFAULT 0,
                    current_balance DECIMAL(15,2) DEFAULT 0,
                    is_active INTEGER DEFAULT 1,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    created_by TEXT
                )",
                
                @"CREATE TABLE IF NOT EXISTS ledger_transactions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    date TEXT NOT NULL,
                    voucher_type TEXT NOT NULL,
                    voucher_number TEXT NOT NULL,
                    ledger_name TEXT NOT NULL,
                    particulars TEXT,
                    debit DECIMAL(15,2) DEFAULT 0,
                    credit DECIMAL(15,2) DEFAULT 0,
                    balance DECIMAL(15,2) DEFAULT 0,
                    reference TEXT,
                    narration TEXT,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    created_by TEXT
                )",
                
                @"CREATE TABLE IF NOT EXISTS receipt_vouchers (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    number TEXT UNIQUE NOT NULL,
                    date TEXT NOT NULL,
                    received_from TEXT NOT NULL,
                    amount_in_words TEXT,
                    amount DECIMAL(15,2) DEFAULT 0,
                    payment_mode TEXT DEFAULT 'Cash',
                    cheque_no TEXT,
                    cheque_date TEXT,
                    bank_name TEXT,
                    narration TEXT,
                    is_posted INTEGER DEFAULT 0,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    created_by TEXT
                )",
                
                @"CREATE TABLE IF NOT EXISTS receipt_details (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    receipt_number TEXT NOT NULL,
                    ledger_name TEXT NOT NULL,
                    particulars TEXT,
                    amount DECIMAL(15,2) DEFAULT 0,
                    voucher_reference TEXT,
                    FOREIGN KEY (receipt_number) REFERENCES receipt_vouchers (number)
                )",
                
                @"CREATE TABLE IF NOT EXISTS payment_vouchers (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    number TEXT UNIQUE NOT NULL,
                    date TEXT NOT NULL,
                    paid_to TEXT NOT NULL,
                    amount_in_words TEXT,
                    amount DECIMAL(15,2) DEFAULT 0,
                    payment_mode TEXT DEFAULT 'Cash',
                    cheque_no TEXT,
                    cheque_date TEXT,
                    bank_name TEXT,
                    narration TEXT,
                    is_posted INTEGER DEFAULT 0,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP,
                    created_by TEXT
                )",
                
                @"CREATE TABLE IF NOT EXISTS payment_details (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    payment_number TEXT NOT NULL,
                    ledger_name TEXT NOT NULL,
                    particulars TEXT,
                    amount DECIMAL(15,2) DEFAULT 0,
                    voucher_reference TEXT,
                    FOREIGN KEY (payment_number) REFERENCES payment_vouchers (number)
                )",
                
                // Audit logs table
                @"CREATE TABLE IF NOT EXISTS audit_logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT NOT NULL,
                    user_role TEXT NOT NULL,
                    action TEXT NOT NULL,
                    entity_type TEXT NOT NULL,
                    entity_id TEXT,
                    details TEXT,
                    old_values TEXT,
                    new_values TEXT,
                    ip_address TEXT,
                    timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
                    module TEXT
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
                // Default admin user
                string userSql = @"INSERT OR IGNORE INTO users (username, password, role, full_name) 
                                   VALUES ('admin', 'admin123', 'Admin', 'Administrator')";
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

                // Default ledgers
                string[] defaultLedgers = {
                    "INSERT OR IGNORE INTO ledgers (name, code, type, opening_balance, balance_type) VALUES ('Cash', '001', 'Cash', 0, 'Dr')",
                    "INSERT OR IGNORE INTO ledgers (name, code, type, opening_balance, balance_type) VALUES ('Bank', '002', 'Bank', 0, 'Dr')",
                    "INSERT OR IGNORE INTO ledgers (name, code, type, opening_balance, balance_type) VALUES ('Sales', '003', 'Income', 0, 'Cr')",
                    "INSERT OR IGNORE INTO ledgers (name, code, type, opening_balance, balance_type) VALUES ('Purchase', '004', 'Expense', 0, 'Dr')",
                    "INSERT OR IGNORE INTO ledgers (name, code, type, opening_balance, balance_type) VALUES ('Sundry Debtors', '005', 'Customer', 0, 'Dr')",
                    "INSERT OR IGNORE INTO ledgers (name, code, type, opening_balance, balance_type) VALUES ('Sundry Creditors', '006', 'Supplier', 0, 'Cr')"
                };

                foreach (string sql in defaultLedgers)
                {
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

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

        // ============ USER MANAGEMENT METHODS ============
        public User GetUserByUsername(string username)
        {
            try
            {
                string sql = "SELECT * FROM users WHERE username = @username AND is_active = 1";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"].ToString(),
                                Password = reader["password"].ToString(),
                                Role = reader["role"].ToString(),
                                FullName = reader["full_name"].ToString(),
                                Email = reader["email"].ToString(),
                                IsActive = Convert.ToBoolean(reader["is_active"]),
                                CreatedDate = DateTime.Parse(reader["created_date"].ToString())
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public bool UpdateUserPassword(string username, string newPassword)
        {
            try
            {
                string sql = "UPDATE users SET password = @password WHERE username = @username";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@password", newPassword);
                    cmd.Parameters.AddWithValue("@username", username);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating password: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            
            try
            {
                string sql = "SELECT * FROM users ORDER BY username";
                using (var cmd = new SQLiteCommand(sql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Username = reader["username"].ToString(),
                            Password = "", // Don't return password
                            Role = reader["role"].ToString(),
                            FullName = reader["full_name"].ToString(),
                            Email = reader["email"].ToString(),
                            IsActive = Convert.ToBoolean(reader["is_active"]),
                            CreatedDate = DateTime.Parse(reader["created_date"].ToString())
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return users;
        }

        public bool AddUser(User user)
        {
            try
            {
                string sql = @"INSERT INTO users (username, password, role, full_name, email) 
                              VALUES (@username, @password, @role, @fullName, @email)";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.Parameters.AddWithValue("@fullName", user.FullName);
                    cmd.Parameters.AddWithValue("@email", user.Email);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding user: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
                    string voucherSql = @"INSERT INTO vouchers (type, number, date, party, amount, description, 
                                  status, reference_voucher, created_by)
                                  VALUES (@type, @number, @date, @party, @amount, @description, 
                                          @status, @reference, @createdBy)";
                    using (var cmd = new SQLiteCommand(voucherSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@type", voucher.Type);
                        cmd.Parameters.AddWithValue("@number", voucher.Number);
                        cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@party", voucher.Party);
                        cmd.Parameters.AddWithValue("@amount", voucher.Amount);
                        cmd.Parameters.AddWithValue("@description", voucher.Description);
                        cmd.Parameters.AddWithValue("@status", voucher.Status);
                        cmd.Parameters.AddWithValue("@reference", voucher.ReferenceVoucher);
			cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);

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
                        // Post to ledger for sales and purchases
			if (voucher.Type == "Sales" || voucher.Type == "Stock Purchase")
			{
			    PostVoucherToLedger(voucher, transaction);
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
                        
                        // Try to extract discount and tax from description
                        string desc = voucher.Description;
                        if (desc.Contains("Discount:"))
                        {
                            // Simple extraction - you might want to improve this
                            var discountPart = desc.Split("Discount:").LastOrDefault()?.Split('%').FirstOrDefault();
                            if (decimal.TryParse(discountPart, out discount)) { }
                        }
                        
                        string estimateSql = @"INSERT INTO estimate_details (estimate_number, discount_percent, tax_percent, expiry_date)
                                           VALUES (@number, @discount, @tax, @expiry)";
                        
                        using (var cmd = new SQLiteCommand(estimateSql, connection, transaction))
                        {
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
                
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@number", voucherNumber);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ParseVoucherFromReader(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading voucher: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return null;
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
                Status = reader["status"].ToString(),
                ReferenceVoucher = reader["reference_voucher"].ToString()
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
                "Receipt" => "RCPT",
                "Payment" => "PAY",
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

        // ============ BACKUP & RESTORE METHODS ============
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

        // ============ UTILITY METHODS ============
        public DataTable ExecuteQuery(string sql)
        {
            var dataTable = new DataTable();
            try
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing query: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        public int ExecuteNonQuery(string sql)
        {
            try
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        // ============ LEDGER MANAGEMENT METHODS ============
        public bool AddLedger(Ledger ledger)
        {
            try
            {
                string sql = @"INSERT INTO ledgers 
                              (name, code, type, contact_person, phone, email, address, gstin,
                               opening_balance, balance_type, credit_limit, current_balance, created_by)
                              VALUES (@name, @code, @type, @contact, @phone, @email, @address, @gstin,
                                      @opening, @balanceType, @creditLimit, @currentBalance, @createdBy)";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", ledger.Name);
                    cmd.Parameters.AddWithValue("@code", ledger.Code);
                    cmd.Parameters.AddWithValue("@type", ledger.Type);
                    cmd.Parameters.AddWithValue("@contact", ledger.ContactPerson);
                    cmd.Parameters.AddWithValue("@phone", ledger.Phone);
                    cmd.Parameters.AddWithValue("@email", ledger.Email);
                    cmd.Parameters.AddWithValue("@address", ledger.Address);
                    cmd.Parameters.AddWithValue("@gstin", ledger.GSTIN);
                    cmd.Parameters.AddWithValue("@opening", ledger.OpeningBalance);
                    cmd.Parameters.AddWithValue("@balanceType", ledger.BalanceType);
                    cmd.Parameters.AddWithValue("@creditLimit", ledger.CreditLimit);
                    cmd.Parameters.AddWithValue("@currentBalance", ledger.CurrentBalance);
                    cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding ledger: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public DataTable GetAllLedgers()
{
    var dataTable = new DataTable();
    
    try
    {
        string sql = @"SELECT l.code, l.name, l.type, l.current_balance, l.balance_type,
                      (SELECT SUM(amount) FROM vouchers v 
                       WHERE v.party = l.name AND v.type = 'Sales' AND v.status = 'Active') as total_sales,
                      (SELECT SUM(amount) FROM vouchers v 
                       WHERE v.party = l.name AND v.type = 'Stock Purchase' AND v.status = 'Active') as total_purchases,
                      l.phone, l.email, l.is_active
                      FROM ledgers l 
                      ORDER BY l.name";
        
        using (var cmd = new SQLiteCommand(sql, connection))
        using (var adapter = new SQLiteDataAdapter(cmd))
        {
            adapter.Fill(dataTable);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading ledgers: {ex.Message}", "Error", 
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    
    return dataTable;
}

        public Ledger GetLedgerByName(string ledgerName)
        {
            try
            {
                string sql = "SELECT * FROM ledgers WHERE name = @name AND is_active = 1";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", ledgerName);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Ledger
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Code = reader["code"].ToString(),
                                Type = reader["type"].ToString(),
                                ContactPerson = reader["contact_person"].ToString(),
                                Phone = reader["phone"].ToString(),
                                Email = reader["email"].ToString(),
                                Address = reader["address"].ToString(),
                                GSTIN = reader["gstin"].ToString(),
                                OpeningBalance = Convert.ToDecimal(reader["opening_balance"]),
                                BalanceType = reader["balance_type"].ToString(),
                                CreditLimit = Convert.ToDecimal(reader["credit_limit"]),
                                CurrentBalance = Convert.ToDecimal(reader["current_balance"]),
                                IsActive = Convert.ToBoolean(reader["is_active"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting ledger: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return null;
        }

        public bool UpdateLedgerBalance(string ledgerName, decimal amount, string transactionType)
        {
            try
            {
                string sql = "";
                if (transactionType == "Dr")
                    sql = "UPDATE ledgers SET current_balance = current_balance + @amount WHERE name = @name";
                else if (transactionType == "Cr")
                    sql = "UPDATE ledgers SET current_balance = current_balance - @amount WHERE name = @name";
                else
                    return false;

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@name", ledgerName);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating ledger balance: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool AddLedgerTransaction(LedgerTransaction transaction)
        {
            try
            {
                string sql = @"INSERT INTO ledger_transactions 
                              (date, voucher_type, voucher_number, ledger_name, particulars,
                               debit, credit, balance, reference, narration, created_by)
                              VALUES (@date, @voucherType, @voucherNumber, @ledgerName, @particulars,
                                      @debit, @credit, @balance, @reference, @narration, @createdBy)";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@date", transaction.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@voucherType", transaction.VoucherType);
                    cmd.Parameters.AddWithValue("@voucherNumber", transaction.VoucherNumber);
                    cmd.Parameters.AddWithValue("@ledgerName", transaction.LedgerName);
                    cmd.Parameters.AddWithValue("@particulars", transaction.Particulars);
                    cmd.Parameters.AddWithValue("@debit", transaction.Debit);
                    cmd.Parameters.AddWithValue("@credit", transaction.Credit);
                    cmd.Parameters.AddWithValue("@balance", transaction.Balance);
                    cmd.Parameters.AddWithValue("@reference", transaction.Reference);
                    cmd.Parameters.AddWithValue("@narration", transaction.Narration);
                    cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding ledger transaction: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public DataTable GetLedgerStatement(string ledgerName, DateTime fromDate, DateTime toDate)
        {
            var dataTable = new DataTable();
            
            try
            {
                string sql = @"SELECT date, voucher_type, voucher_number, particulars, 
                              debit, credit, balance, reference
                              FROM ledger_transactions 
                              WHERE ledger_name = @ledgerName 
                              AND DATE(date) BETWEEN @fromDate AND @toDate
                              ORDER BY date, id";
                
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ledgerName", ledgerName);
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
                MessageBox.Show($"Error loading ledger statement: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return dataTable;
        }

        public bool AddReceiptVoucher(ReceiptVoucher voucher)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Insert receipt voucher
                    string voucherSql = @"INSERT INTO receipt_vouchers 
                                        (number, date, received_from, amount_in_words, amount,
                                         payment_mode, cheque_no, cheque_date, bank_name, narration, created_by)
                                        VALUES (@number, @date, @receivedFrom, @amountInWords, @amount,
                                                @paymentMode, @chequeNo, @chequeDate, @bankName, @narration, @createdBy)";

                    using (var cmd = new SQLiteCommand(voucherSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@number", voucher.Number);
                        cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@receivedFrom", voucher.ReceivedFrom);
                        cmd.Parameters.AddWithValue("@amountInWords", voucher.AmountInWords);
                        cmd.Parameters.AddWithValue("@amount", voucher.Amount);
                        cmd.Parameters.AddWithValue("@paymentMode", voucher.PaymentMode);
                        cmd.Parameters.AddWithValue("@chequeNo", voucher.ChequeNo);
                        cmd.Parameters.AddWithValue("@chequeDate", voucher.ChequeDate?.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@bankName", voucher.BankName);
                        cmd.Parameters.AddWithValue("@narration", voucher.Narration);
                        cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);

                        if (cmd.ExecuteNonQuery() == 0)
                            throw new Exception("Failed to insert receipt voucher");
                    }

                    // Insert receipt details
                    foreach (var detail in voucher.Details)
                    {
                        string detailSql = @"INSERT INTO receipt_details 
                                           (receipt_number, ledger_name, particulars, amount, voucher_reference)
                                           VALUES (@receiptNumber, @ledgerName, @particulars, @amount, @voucherReference)";

                        using (var cmd = new SQLiteCommand(detailSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@receiptNumber", voucher.Number);
                            cmd.Parameters.AddWithValue("@ledgerName", detail.LedgerName);
                            cmd.Parameters.AddWithValue("@particulars", detail.Particulars);
                            cmd.Parameters.AddWithValue("@amount", detail.Amount);
                            cmd.Parameters.AddWithValue("@voucherReference", detail.VoucherReference);

                            if (cmd.ExecuteNonQuery() == 0)
                                throw new Exception("Failed to insert receipt detail");
                        }
                    }

                    // Post to ledger transactions
                    PostReceiptToLedger(voucher, transaction);

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Error adding receipt voucher: {ex.Message}", "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private void PostReceiptToLedger(ReceiptVoucher voucher, SQLiteTransaction transaction)
        {
            try
            {
                // Debit Cash/Bank (increase asset)
                string debitLedger = voucher.PaymentMode == "Cash" ? "Cash" : "Bank";
                
                // Get current balance
                decimal currentBalance = GetLedgerCurrentBalance(debitLedger);
                decimal newBalance = currentBalance + voucher.Amount;
                
                // Add debit transaction
                string debitSql = @"INSERT INTO ledger_transactions 
                                  (date, voucher_type, voucher_number, ledger_name, particulars,
                                   debit, credit, balance, reference, narration, created_by)
                                  VALUES (@date, 'Receipt', @voucherNumber, @ledgerName, @particulars,
                                          @debit, @credit, @balance, @reference, @narration, @createdBy)";
                
                using (var cmd = new SQLiteCommand(debitSql, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@voucherNumber", voucher.Number);
                    cmd.Parameters.AddWithValue("@ledgerName", debitLedger);
                    cmd.Parameters.AddWithValue("@particulars", $"Received from {voucher.ReceivedFrom}");
                    cmd.Parameters.AddWithValue("@debit", voucher.Amount);
                    cmd.Parameters.AddWithValue("@credit", 0);
                    cmd.Parameters.AddWithValue("@balance", newBalance);
                    cmd.Parameters.AddWithValue("@reference", voucher.Number);
                    cmd.Parameters.AddWithValue("@narration", voucher.Narration);
                    cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);
                    
                    cmd.ExecuteNonQuery();
                }
                
                // Update ledger balance
                UpdateLedgerBalance(debitLedger, voucher.Amount, "Dr");
                
                // Credit respective ledgers from details
                foreach (var detail in voucher.Details)
                {
                    currentBalance = GetLedgerCurrentBalance(detail.LedgerName);
                    newBalance = currentBalance - voucher.Amount;
                    
                    // Add credit transaction
                    using (var cmd = new SQLiteCommand(debitSql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@voucherNumber", voucher.Number);
                        cmd.Parameters.AddWithValue("@ledgerName", detail.LedgerName);
                        cmd.Parameters.AddWithValue("@particulars", detail.Particulars);
                        cmd.Parameters.AddWithValue("@debit", 0);
                        cmd.Parameters.AddWithValue("@credit", detail.Amount);
                        cmd.Parameters.AddWithValue("@balance", newBalance);
                        cmd.Parameters.AddWithValue("@reference", detail.VoucherReference);
                        cmd.Parameters.AddWithValue("@narration", voucher.Narration);
                        cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);
                        
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Update ledger balance
                    UpdateLedgerBalance(detail.LedgerName, detail.Amount, "Cr");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error posting to ledger: {ex.Message}");
            }
        }

        private decimal GetLedgerCurrentBalance(string ledgerName)
        {
            try
            {
                string sql = "SELECT current_balance FROM ledgers WHERE name = @name";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@name", ledgerName);
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch { return 0; }
        }

        public DataTable GetReceiptVouchers()
        {
            var dataTable = new DataTable();
            
            try
            {
                string sql = @"SELECT number, date, received_from, amount, payment_mode, 
                              CASE WHEN is_posted = 1 THEN 'Posted' ELSE 'Draft' END as status
                              FROM receipt_vouchers 
                              ORDER BY date DESC";
                
                using (var cmd = new SQLiteCommand(sql, connection))
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading receipts: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return dataTable;
        }

        // ============ LEDGER POSTING METHODS ============
        private void PostVoucherToLedger(Voucher voucher, SQLiteTransaction transaction)
        {
            try
            {
                if (voucher.Type == "Sales")
                {
                    // For Sales: Debit Party (Customer), Credit Sales
                    PostSalesTransaction(voucher, transaction);
                }
                else if (voucher.Type == "Stock Purchase")
                {
                    // For Purchase: Debit Purchase, Credit Party (Supplier)
                    PostPurchaseTransaction(voucher, transaction);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error posting voucher to ledger: {ex.Message}");
            }
        }

        private void PostSalesTransaction(Voucher voucher, SQLiteTransaction transaction)
        {
            // Debit Customer/Sundry Debtors
            string debitLedger = "Sundry Debtors"; // Default
            if (!string.IsNullOrWhiteSpace(voucher.Party))
            {
                // Check if party exists as ledger
                var partyLedger = GetLedgerByName(voucher.Party);
                if (partyLedger != null)
                {
                    debitLedger = voucher.Party;
                }
            }
            
            // Credit Sales Account
            string creditLedger = "Sales";
            
            // Debit transaction for Customer
            decimal currentBalance = GetLedgerCurrentBalance(debitLedger);
            decimal newBalance = currentBalance + voucher.Amount;
            
            string ledgerSql = @"INSERT INTO ledger_transactions 
                        (date, voucher_type, voucher_number, ledger_name, particulars,
                         debit, credit, balance, reference, narration, created_by)
                        VALUES (@date, 'Sales', @voucherNumber, @ledgerName, @particulars,
                                @debit, @credit, @balance, @reference, @narration, @createdBy)";
            
            using (var cmd = new SQLiteCommand(ledgerSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@voucherNumber", voucher.Number);
                cmd.Parameters.AddWithValue("@ledgerName", debitLedger);
                cmd.Parameters.AddWithValue("@particulars", $"Sales to {voucher.Party}");
                cmd.Parameters.AddWithValue("@debit", voucher.Amount);
                cmd.Parameters.AddWithValue("@credit", 0);
                cmd.Parameters.AddWithValue("@balance", newBalance);
                cmd.Parameters.AddWithValue("@reference", voucher.Number);
                cmd.Parameters.AddWithValue("@narration", voucher.Description);
                cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);
                
                cmd.ExecuteNonQuery();
            }
            
            // Update ledger balance
            UpdateLedgerBalance(debitLedger, voucher.Amount, "Dr");
            
            // Credit transaction for Sales
            currentBalance = GetLedgerCurrentBalance(creditLedger);
            newBalance = currentBalance - voucher.Amount;
            
            using (var cmd = new SQLiteCommand(ledgerSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@voucherNumber", voucher.Number);
                cmd.Parameters.AddWithValue("@ledgerName", creditLedger);
                cmd.Parameters.AddWithValue("@particulars", $"Sales Invoice {voucher.Number}");
                cmd.Parameters.AddWithValue("@debit", 0);
                cmd.Parameters.AddWithValue("@credit", voucher.Amount);
                cmd.Parameters.AddWithValue("@balance", newBalance);
                cmd.Parameters.AddWithValue("@reference", voucher.Number);
                cmd.Parameters.AddWithValue("@narration", voucher.Description);
                cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);
                
                cmd.ExecuteNonQuery();
            }
            
            // Update ledger balance
            UpdateLedgerBalance(creditLedger, voucher.Amount, "Cr");
        }

        private void PostPurchaseTransaction(Voucher voucher, SQLiteTransaction transaction)
        {
            // Debit Purchase Account
            string debitLedger = "Purchase";
            
            // Credit Supplier/Sundry Creditors
            string creditLedger = "Sundry Creditors"; // Default
            if (!string.IsNullOrWhiteSpace(voucher.Party))
            {
                // Check if party exists as ledger
                var partyLedger = GetLedgerByName(voucher.Party);
                if (partyLedger != null)
                {
                    creditLedger = voucher.Party;
                }
            }
            
            // Debit transaction for Purchase
            decimal currentBalance = GetLedgerCurrentBalance(debitLedger);
            decimal newBalance = currentBalance + voucher.Amount;
            
            string ledgerSql = @"INSERT INTO ledger_transactions 
                        (date, voucher_type, voucher_number, ledger_name, particulars,
                         debit, credit, balance, reference, narration, created_by)
                        VALUES (@date, 'Purchase', @voucherNumber, @ledgerName, @particulars,
                                @debit, @credit, @balance, @reference, @narration, @createdBy)";
            
            using (var cmd = new SQLiteCommand(ledgerSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@voucherNumber", voucher.Number);
                cmd.Parameters.AddWithValue("@ledgerName", debitLedger);
                cmd.Parameters.AddWithValue("@particulars", $"Purchase from {voucher.Party}");
                cmd.Parameters.AddWithValue("@debit", voucher.Amount);
                cmd.Parameters.AddWithValue("@credit", 0);
                cmd.Parameters.AddWithValue("@balance", newBalance);
                cmd.Parameters.AddWithValue("@reference", voucher.Number);
                cmd.Parameters.AddWithValue("@narration", voucher.Description);
                cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);
                
                cmd.ExecuteNonQuery();
            }
            
            // Update ledger balance
            UpdateLedgerBalance(debitLedger, voucher.Amount, "Dr");
            
            // Credit transaction for Supplier
            currentBalance = GetLedgerCurrentBalance(creditLedger);
            newBalance = currentBalance - voucher.Amount;
            
            using (var cmd = new SQLiteCommand(ledgerSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@date", voucher.Date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@voucherNumber", voucher.Number);
                cmd.Parameters.AddWithValue("@ledgerName", creditLedger);
                cmd.Parameters.AddWithValue("@particulars", $"Purchase Invoice {voucher.Number}");
                cmd.Parameters.AddWithValue("@debit", 0);
                cmd.Parameters.AddWithValue("@credit", voucher.Amount);
                cmd.Parameters.AddWithValue("@balance", newBalance);
                cmd.Parameters.AddWithValue("@reference", voucher.Number);
                cmd.Parameters.AddWithValue("@narration", voucher.Description);
                cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);
                
                cmd.ExecuteNonQuery();
            }
            
            // Update ledger balance
            UpdateLedgerBalance(creditLedger, voucher.Amount, "Cr");
        }

        public void Dispose()
        {
            CloseConnection();
            connection?.Dispose();
        }
    }
}