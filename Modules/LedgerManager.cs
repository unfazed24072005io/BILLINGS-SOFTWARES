using System;
using System.Data.SQLite;
using System.Collections.Generic;
using BillingSoftware.Models;

namespace BillingSoftware.Modules
{
    public class LedgerManager
    {
        private DatabaseManager dbManager;
        
        public LedgerManager()
        {
            dbManager = new DatabaseManager();
        }
        
        // Post Sales Transaction to Ledger
        public bool PostSalesTransaction(Voucher voucher)
        {
            try
            {
                using (var transaction = dbManager.GetConnection().BeginTransaction())
                {
                    // Debit: Customer/Sundry Debtors
                    PostToLedger("Sundry Debtors", voucher.Party, voucher.Number, 
                                voucher.Amount, 0, voucher.Date, "SALES", 
                                $"Sales to {voucher.Party}", transaction);
                    
                    // Credit: Sales Account
                    PostToLedger("Sales", "", voucher.Number, 
                                0, voucher.Amount, voucher.Date, "SALES", 
                                $"Sales Invoice {voucher.Number}", transaction);
                    
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting sales to ledger: {ex.Message}");
                return false;
            }
        }
        
        // Post Purchase Transaction to Ledger
        public bool PostPurchaseTransaction(Voucher voucher)
        {
            try
            {
                using (var transaction = dbManager.GetConnection().BeginTransaction())
                {
                    // Debit: Purchase Account
                    PostToLedger("Purchase", "", voucher.Number, 
                                voucher.Amount, 0, voucher.Date, "PURCHASE", 
                                $"Purchase from {voucher.Party}", transaction);
                    
                    // Credit: Supplier/Sundry Creditors
                    PostToLedger("Sundry Creditors", voucher.Party, voucher.Number, 
                                0, voucher.Amount, voucher.Date, "PURCHASE", 
                                $"Purchase Invoice {voucher.Number}", transaction);
                    
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting purchase to ledger: {ex.Message}");
                return false;
            }
        }
        
        // Post Receipt Transaction to Ledger
        public bool PostReceiptTransaction(ReceiptVoucher receipt)
        {
            try
            {
                using (var transaction = dbManager.GetConnection().BeginTransaction())
                {
                    // Debit: Cash/Bank (increase)
                    string cashBankLedger = receipt.PaymentMode == "Cash" ? "Cash" : "Bank";
                    
                    PostToLedger(cashBankLedger, "", receipt.Number, 
                                receipt.Amount, 0, receipt.Date, "RECEIPT", 
                                $"Received from {receipt.ReceivedFrom}", transaction);
                    
                    // Credit: Sundry Debtors (decrease)
                    PostToLedger("Sundry Debtors", receipt.ReceivedFrom, receipt.Number, 
                                0, receipt.Amount, receipt.Date, "RECEIPT", 
                                $"Receipt against {receipt.ReceivedFrom}", transaction);
                    
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting receipt to ledger: {ex.Message}");
                return false;
            }
        }
        
        // Post Payment Transaction to Ledger
        public bool PostPaymentTransaction(PaymentVoucher payment)
        {
            try
            {
                using (var transaction = dbManager.GetConnection().BeginTransaction())
                {
                    // Debit: Sundry Creditors (decrease)
                    PostToLedger("Sundry Creditors", payment.PaidTo, payment.Number, 
                                payment.Amount, 0, payment.Date, "PAYMENT", 
                                $"Payment to {payment.PaidTo}", transaction);
                    
                    // Credit: Cash/Bank (decrease)
                    string cashBankLedger = payment.PaymentMode == "Cash" ? "Cash" : "Bank";
                    
                    PostToLedger(cashBankLedger, "", payment.Number, 
                                0, payment.Amount, payment.Date, "PAYMENT", 
                                $"Payment to {payment.PaidTo}", transaction);
                    
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error posting payment to ledger: {ex.Message}");
                return false;
            }
        }
        
        private void PostToLedger(string ledgerName, string subLedger, string voucherNumber,
                                 decimal debit, decimal credit, DateTime date, 
                                 string transactionType, string particulars, SQLiteTransaction trans)
        {
            try
            {
                // Get current balance
                decimal currentBalance = GetLedgerBalance(ledgerName, subLedger);
                
                // Calculate new balance based on balance type
                string balanceType = GetLedgerBalanceType(ledgerName);
                decimal newBalance = CalculateNewBalance(currentBalance, debit, credit, balanceType);
                
                // Insert ledger transaction
                string sql = @"INSERT INTO ledger_transactions 
                              (date, transaction_type, voucher_number, ledger_name, sub_ledger,
                               particulars, debit, credit, balance, created_by)
                              VALUES (@date, @transactionType, @voucherNumber, @ledgerName, @subLedger,
                                      @particulars, @debit, @credit, @balance, @createdBy)";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection(), trans))
                {
                    cmd.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@transactionType", transactionType);
                    cmd.Parameters.AddWithValue("@voucherNumber", voucherNumber);
                    cmd.Parameters.AddWithValue("@ledgerName", ledgerName);
                    cmd.Parameters.AddWithValue("@subLedger", subLedger);
                    cmd.Parameters.AddWithValue("@particulars", particulars);
                    cmd.Parameters.AddWithValue("@debit", debit);
                    cmd.Parameters.AddWithValue("@credit", credit);
                    cmd.Parameters.AddWithValue("@balance", newBalance);
                    cmd.Parameters.AddWithValue("@createdBy", Program.CurrentUser);
                    
                    cmd.ExecuteNonQuery();
                }
                
                // Update ledger master balance
                UpdateLedgerMasterBalance(ledgerName, subLedger, newBalance, trans);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error posting to ledger: {ex.Message}");
            }
        }
        
        private decimal GetLedgerBalance(string ledgerName, string subLedger = "")
        {
            try
            {
                string sql = "SELECT current_balance FROM ledgers WHERE name = @name";
                if (!string.IsNullOrEmpty(subLedger))
                {
                    sql = @"SELECT COALESCE(SUM(balance), 0) 
                           FROM ledger_transactions 
                           WHERE ledger_name = @name AND sub_ledger = @subLedger
                           ORDER BY id DESC LIMIT 1";
                }
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@name", ledgerName);
                    if (!string.IsNullOrEmpty(subLedger))
                        cmd.Parameters.AddWithValue("@subLedger", subLedger);
                    
                    object result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
            catch { return 0; }
        }
        
        private string GetLedgerBalanceType(string ledgerName)
        {
            try
            {
                string sql = "SELECT balance_type FROM ledgers WHERE name = @name";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@name", ledgerName);
                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Dr";
                }
            }
            catch { return "Dr"; }
        }
        
        private decimal CalculateNewBalance(decimal currentBalance, decimal debit, 
                                          decimal credit, string balanceType)
        {
            if (balanceType == "Dr")
            {
                return currentBalance + debit - credit;
            }
            else // Cr balance type
            {
                return currentBalance + credit - debit;
            }
        }
        
        private void UpdateLedgerMasterBalance(string ledgerName, string subLedger, 
                                              decimal newBalance, SQLiteTransaction trans)
        {
            try
            {
                string sql = @"UPDATE ledgers SET current_balance = @balance 
                              WHERE name = @name";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection(), trans))
                {
                    cmd.Parameters.AddWithValue("@balance", newBalance);
                    cmd.Parameters.AddWithValue("@name", ledgerName);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* Silently fail if ledger doesn't exist yet */ }
        }
        
        // Get ledger statement with proper balance calculations
        public System.Data.DataTable GetLedgerStatement(string ledgerName, DateTime fromDate, DateTime toDate)
        {
            var dataTable = new System.Data.DataTable();
            
            try
            {
                string sql = @"SELECT 
                              date,
                              transaction_type as 'Type',
                              voucher_number as 'Voucher No',
                              sub_ledger as 'Party',
                              particulars as 'Particulars',
                              debit as 'Debit',
                              credit as 'Credit',
                              balance as 'Balance',
                              narration as 'Narration'
                              FROM ledger_transactions 
                              WHERE ledger_name = @ledgerName 
                              AND DATE(date) BETWEEN @fromDate AND @toDate
                              ORDER BY date, id";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
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
                Console.WriteLine($"Error getting ledger statement: {ex.Message}");
            }
            
            return dataTable;
        }
    }
}