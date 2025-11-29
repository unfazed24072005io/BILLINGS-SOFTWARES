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

                // Products table
                @"CREATE TABLE IF NOT EXISTS products (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    code TEXT UNIQUE NOT NULL,
                    price DECIMAL(15,2) DEFAULT 0,
                    stock DECIMAL(15,3) DEFAULT 0,
                    unit TEXT DEFAULT 'PCS',
                    min_stock DECIMAL(15,3) DEFAULT 0,
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
                )"
            };

            foreach (string table in tables)
            {
                using (var cmd = new SQLiteCommand(table, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // Insert default data
            InsertDefaultData();
        }

        private void InsertDefaultData()
        {
            // Default admin user
            string adminSql = @"INSERT OR IGNORE INTO users (username, password, role) 
                               VALUES ('admin', 'admin', 'Admin')";
            using (var cmd = new SQLiteCommand(adminSql, connection))
                cmd.ExecuteNonQuery();

            // Default company settings
            string settingsSql = @"INSERT OR IGNORE INTO company_settings (company_name) 
                                  VALUES ('My Billing Company')";
            using (var cmd = new SQLiteCommand(settingsSql, connection))
                cmd.ExecuteNonQuery();
        }

        public SQLiteConnection GetConnection()
        {
            return connection;
        }

        public void CloseConnection()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
    }
}