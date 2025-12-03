using System;
using System.Data.SQLite;
using System.Net;
using System.Windows.Forms;
using BillingSoftware.Models;

namespace BillingSoftware.Modules
{
    public class AuditLogger
    {
        private DatabaseManager dbManager;
        
        public AuditLogger()
        {
            dbManager = new DatabaseManager();
            CreateAuditTable();
        }
        
        private void CreateAuditTable()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS audit_logs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL,
                user_role TEXT NOT NULL,
                action TEXT NOT NULL, -- CREATE, UPDATE, DELETE, VIEW, PRINT, EXPORT
                entity_type TEXT NOT NULL, -- VOUCHER, PRODUCT, USER, LEDGER
                entity_id TEXT,
                details TEXT,
                old_values TEXT,
                new_values TEXT,
                ip_address TEXT,
                timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
                module TEXT
            )";
            
            using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
            {
                cmd.ExecuteNonQuery();
            }
        }
        
        public void LogAction(string action, string entityType, string entityId, 
                            string details, string module, string oldValues = "", 
                            string newValues = "")
        {
            try
            {
                string sql = @"INSERT INTO audit_logs 
                              (username, user_role, action, entity_type, entity_id, 
                               details, old_values, new_values, ip_address, module)
                              VALUES (@username, @role, @action, @entityType, @entityId, 
                                      @details, @oldValues, @newValues, @ip, @module)";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@username", Program.CurrentUser);
                    cmd.Parameters.AddWithValue("@role", Program.UserRole);
                    cmd.Parameters.AddWithValue("@action", action);
                    cmd.Parameters.AddWithValue("@entityType", entityType);
                    cmd.Parameters.AddWithValue("@entityId", entityId);
                    cmd.Parameters.AddWithValue("@details", details);
                    cmd.Parameters.AddWithValue("@oldValues", oldValues);
                    cmd.Parameters.AddWithValue("@newValues", newValues);
                    cmd.Parameters.AddWithValue("@ip", GetLocalIPAddress());
                    cmd.Parameters.AddWithValue("@module", module);
                    
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Silent fail - don't break the application if audit logging fails
                Console.WriteLine($"Audit logging error: {ex.Message}");
            }
        }
        
        private string GetLocalIPAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                var addresses = Dns.GetHostAddresses(hostName);
                foreach (var ip in addresses)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }
        
        public System.Data.DataTable GetAuditLogs(DateTime fromDate, DateTime toDate, 
                                                string username = "", string action = "")
        {
            var dataTable = new System.Data.DataTable();
            
            try
            {
                string sql = @"SELECT timestamp, username, user_role, action, 
                              entity_type, entity_id, details, module
                              FROM audit_logs 
                              WHERE DATE(timestamp) BETWEEN @fromDate AND @toDate ";
                
                if (!string.IsNullOrEmpty(username))
                    sql += " AND username = @username";
                if (!string.IsNullOrEmpty(action))
                    sql += " AND action = @action";
                    
                sql += " ORDER BY timestamp DESC";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"));
                    
                    if (!string.IsNullOrEmpty(username))
                        cmd.Parameters.AddWithValue("@username", username);
                    if (!string.IsNullOrEmpty(action))
                        cmd.Parameters.AddWithValue("@action", action);
                    
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audit logs: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return dataTable;
        }
    }
}