using System;
using System.Data.SQLite;
using System.Windows.Forms;
using BillingSoftware.Models;

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
                // Vouchers table with reference numbers
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
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP
                )",

                // Voucher Items table for detailed item tracking
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

                // Users table
                @"CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    role TEXT DEFAULT 'User',
                    is_active INTEGER DEFAULT 1,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP
                )",

                // Company settings table
                @"CREATE TABLE IF NOT EXISTS company_settings (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    company_name TEXT DEFAULT 'My Company',
                    address TEXT,
                    phone TEXT,
                    email TEXT,
                    gst_number TEXT,
                    currency TEXT DEFAULT '₹'
                )",

                // Stock transactions table for detailed tracking
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
                    MessageBox.Show($"Error creating table: {ex.Message}", "Database Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                string adminSql = @"INSERT OR IGNORE INTO users (username, password, role) 
                                   VALUES ('admin', 'admin', 'Admin')";
                using (var cmd = new SQLiteCommand(adminSql, connection))
                    cmd.ExecuteNonQuery();

                // Default company settings
                string settingsSql = @"INSERT OR IGNORE INTO company_settings (id, company_name, address, phone, email, gst_number, currency) 
                                      VALUES (1, 'My Billing Company', '123 Business Street', '9876543210', 'info@mycompany.com', 'GST123456789', '₹')";
                using (var cmd = new SQLiteCommand(settingsSql, connection))
                    cmd.ExecuteNonQuery();

                // Sample products for demonstration
                string productsSql = @"INSERT OR IGNORE INTO products (name, code, price, stock, unit, min_stock) VALUES
                                      ('Laptop Dell XPS', 'LP001', 75000, 15, 'PCS', 5),
                                      ('Wireless Mouse Logitech', 'MS002', 1200, 30, 'PCS', 10),
                                      ('Mechanical Keyboard', 'KB001', 3500, 20, 'PCS', 8),
                                      ('27-inch Monitor', 'MN001', 22000, 8, 'PCS', 3),
                                      ('USB-C Cable', 'UC001', 800, 50, 'PCS', 20),
                                      ('Laptop Bag', 'BG001', 1500, 25, 'PCS', 5),
                                      ('Wireless Headphones', 'HP001', 4500, 12, 'PCS', 6),
                                      ('Webcam HD', 'WC001', 3200, 18, 'PCS', 4)";
                using (var cmd = new SQLiteCommand(productsSql, connection))
                    cmd.ExecuteNonQuery();

                // Insert sample sales data for testing
                InsertSampleSalesData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting default data: {ex.Message}", "Database Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InsertSampleSalesData()
        {
            try
            {
                // Insert sample sales vouchers for testing
                string salesVouchersSql = @"INSERT OR IGNORE INTO vouchers (type, number, date, party, amount, description, status) VALUES
                                           ('Sales', 'SL-001', '2024-01-15', 'ABC Corporation', 82000, 'Laptop and accessories sale', 'Active'),
                                           ('Sales', 'SL-002', '2024-01-18', 'XYZ Enterprises', 45000, 'Monitor and keyboard sale', 'Active'),
                                           ('Sales', 'SL-003', '2024-01-20', 'Individual Customer', 12000, 'Wireless accessories', 'Active')";
                using (var cmd = new SQLiteCommand(salesVouchersSql, connection))
                    cmd.ExecuteNonQuery();

                // Insert sample sales items
                string salesItemsSql = @"INSERT OR IGNORE INTO voucher_items (voucher_number, product_name, quantity, unit_price, total_amount) VALUES
                                        ('SL-001', 'Laptop Dell XPS', 1, 75000, 75000),
                                        ('SL-001', 'Wireless Mouse Logitech', 1, 1200, 1200),
                                        ('SL-001', 'Laptop Bag', 1, 1500, 1500),
                                        ('SL-001', 'USB-C Cable', 2, 800, 1600),
                                        ('SL-002', '27-inch Monitor', 2, 22000, 44000),
                                        ('SL-002', 'Mechanical Keyboard', 1, 3500, 3500),
                                        ('SL-003', 'Wireless Headphones', 2, 4500, 9000),
                                        ('SL-003', 'Webcam HD', 1, 3200, 3200)";
                using (var cmd = new SQLiteCommand(salesItemsSql, connection))
                    cmd.ExecuteNonQuery();

                // Insert sample purchase vouchers
                string purchaseVouchersSql = @"INSERT OR IGNORE INTO vouchers (type, number, date, party, amount, description, status) VALUES
                                              ('Stock Purchase', 'STK-001', '2024-01-10', 'Tech Suppliers Ltd', 150000, 'Bulk laptop purchase', 'Active'),
                                              ('Stock Purchase', 'STK-002', '2024-01-12', 'Accessories Wholesale', 50000, 'Accessories stock', 'Active')";
                using (var cmd = new SQLiteCommand(purchaseVouchersSql, connection))
                    cmd.ExecuteNonQuery();

                // Insert sample purchase items
                string purchaseItemsSql = @"INSERT OR IGNORE INTO voucher_items (voucher_number, product_name, quantity, unit_price, total_amount) VALUES
                                           ('STK-001', 'Laptop Dell XPS', 2, 70000, 140000),
                                           ('STK-001', '27-inch Monitor', 3, 20000, 60000),
                                           ('STK-002', 'Wireless Mouse Logitech', 20, 1000, 20000),
                                           ('STK-002', 'Mechanical Keyboard', 10, 3000, 30000)";
                using (var cmd = new SQLiteCommand(purchaseItemsSql, connection))
                    cmd.ExecuteNonQuery();

                // Insert sample receipt against sales
                string receiptSql = @"INSERT OR IGNORE INTO vouchers (type, number, date, party, amount, description, status, reference_voucher) VALUES
                                     ('Receipt', 'RCPT-001', '2024-01-16', 'ABC Corporation', 82000, 'Payment against invoice SL-001', 'Active', 'SL-001'),
                                     ('Receipt', 'RCPT-002', '2024-01-22', 'Individual Customer', 12000, 'Full payment received', 'Active', 'SL-003')";
                using (var cmd = new SQLiteCommand(receiptSql, connection))
                    cmd.ExecuteNonQuery();

                // Insert sample payment against purchases
                string paymentSql = @"INSERT OR IGNORE INTO vouchers (type, number, date, party, amount, description, status, reference_voucher) VALUES
                                     ('Payment', 'PAY-001', '2024-01-11', 'Tech Suppliers Ltd', 100000, 'Advance payment', 'Active', 'STK-001'),
                                     ('Payment', 'PAY-002', '2024-01-13', 'Accessories Wholesale', 50000, 'Full payment', 'Active', 'STK-002')";
                using (var cmd = new SQLiteCommand(paymentSql, connection))
                    cmd.ExecuteNonQuery();

                // Update product stock based on sample data
                UpdateProductStockFromSampleData();
            }
            catch (Exception ex)
            {
                // Silently continue - sample data is optional
                Console.WriteLine($"Sample data insertion skipped: {ex.Message}");
            }
        }

        private void UpdateProductStockFromSampleData()
        {
            try
            {
                // Update stock based on sample purchases and sales
                string updateStockSql = @"UPDATE products SET stock = 
                                        CASE 
                                            WHEN name = 'Laptop Dell XPS' THEN 15
                                            WHEN name = 'Wireless Mouse Logitech' THEN 30
                                            WHEN name = 'Mechanical Keyboard' THEN 20
                                            WHEN name = '27-inch Monitor' THEN 8
                                            WHEN name = 'USB-C Cable' THEN 50
                                            WHEN name = 'Laptop Bag' THEN 25
                                            WHEN name = 'Wireless Headphones' THEN 12
                                            WHEN name = 'Webcam HD' THEN 18
                                            ELSE stock
                                        END";
                using (var cmd = new SQLiteCommand(updateStockSql, connection))
                    cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stock update skipped: {ex.Message}");
            }
        }

        public SQLiteConnection GetConnection()
        {
            // Ensure connection is open
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

        // Method to add stock transaction
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

        // Method to update product stock
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

        // Method to get all products for combobox
        public System.Data.DataTable GetProductsForComboBox()
        {
            var dataTable = new System.Data.DataTable();
            try
            {
                string sql = "SELECT name, price, stock FROM products WHERE stock > 0 ORDER BY name";
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

        // Method to check if voucher number exists
        public bool VoucherExists(string voucherNumber)
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM vouchers WHERE number = @voucherNumber";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@voucherNumber", voucherNumber);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking voucher: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Method to get voucher details
        public Voucher GetVoucherByNumber(string voucherNumber)
        {
            try
            {
                string sql = @"SELECT * FROM vouchers WHERE number = @voucherNumber";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@voucherNumber", voucherNumber);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Voucher
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting voucher: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        // Method to backup database
        public bool BackupDatabase(string backupPath)
        {
            try
            {
                CloseConnection(); // Close current connection
                System.IO.File.Copy("billing.db", backupPath, true);
                InitializeDatabase(); // Reopen connection
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Method to restore database
        public bool RestoreDatabase(string backupPath)
        {
            try
            {
                var result = MessageBox.Show("This will replace all current data. Continue?", "Confirm Restore", 
                                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    CloseConnection(); // Close current connection
                    System.IO.File.Copy(backupPath, "billing.db", true);
                    InitializeDatabase(); // Reopen connection with restored data
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

        // Method to get database statistics
        public string GetDatabaseStats()
        {
            try
            {
                string stats = "";
                
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

                // Count products
                string productSql = "SELECT COUNT(*) as count FROM products";
                using (var cmd = new SQLiteCommand(productSql, connection))
                {
                    stats += $"Products: {cmd.ExecuteScalar()}\n";
                }

                // Total sales
                string salesSql = "SELECT SUM(amount) as total FROM vouchers WHERE type = 'Sales' AND status = 'Active'";
                using (var cmd = new SQLiteCommand(salesSql, connection))
                {
                    var result = cmd.ExecuteScalar();
                    decimal totalSales = result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                    stats += $"Total Sales: ₹{totalSales:N2}\n";
                }

                return stats;
            }
            catch (Exception ex)
            {
                return $"Error getting stats: {ex.Message}";
            }
        }

        // Dispose method
        public void Dispose()
        {
            CloseConnection();
            connection?.Dispose();
        }
    }
}