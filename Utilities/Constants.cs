namespace BillingSoftware.Utilities
{
    public static class Constants
    {
        // Application Information
        public const string APP_NAME = "Modern Billing Software";
        public const string APP_VERSION = "1.0.0";
        public const string APP_DEVELOPER = "Your Company Name";
        public const string APP_COPYRIGHT = "© 2024 Your Company Name. All rights reserved.";

        // Database Constants
        public const string DATABASE_NAME = "billing.db";
        public const string DATABASE_BACKUP_EXTENSION = ".backup";
        
        // Voucher Types
        public static class VoucherTypes
        {
            public const string SALES = "Sales";
            public const string RECEIPT = "Receipt";
            public const string PAYMENT = "Payment";
            public const string JOURNAL = "Journal";
            public const string ESTIMATE = "Estimate";

            public static readonly string[] ALL_TYPES = { SALES, RECEIPT, PAYMENT, JOURNAL, ESTIMATE };
        }

        // Voucher Prefixes
        public static class VoucherPrefixes
        {
            public const string SALES = "SL";
            public const string RECEIPT = "RCPT";
            public const string PAYMENT = "PAY";
            public const string JOURNAL = "JRN";
            public const string ESTIMATE = "EST";
        }

        // Voucher Status
        public static class VoucherStatus
        {
            public const string ACTIVE = "Active";
            public const string CANCELLED = "Cancelled";
            public const string DRAFT = "Draft";
            public const string COMPLETED = "Completed";
        }

        // User Roles
        public static class UserRoles
        {
            public const string ADMIN = "Admin";
            public const string USER = "User";
            public const string MANAGER = "Manager";

            public static readonly string[] ALL_ROLES = { ADMIN, USER, MANAGER };
        }

        // Payment Modes
        public static class PaymentModes
        {
            public const string CASH = "Cash";
            public const string BANK_TRANSFER = "Bank Transfer";
            public const string CHEQUE = "Cheque";
            public const string CARD = "Card";
            public const string UPI = "UPI";
            public const string ONLINE = "Online";

            public static readonly string[] ALL_MODES = { CASH, BANK_TRANSFER, CHEQUE, CARD, UPI, ONLINE };
        }

        // Report Types
        public static class ReportTypes
        {
            public const string DAILY = "Daily";
            public const string WEEKLY = "Weekly";
            public const string MONTHLY = "Monthly";
            public const string YEARLY = "Yearly";
            public const string CUSTOM = "Custom";

            // Stock Reports
            public const string CURRENT_STOCK = "Current Stock";
            public const string LOW_STOCK = "Low Stock";
            public const string STOCK_VALUATION = "Stock Valuation";

            // Financial Reports
            public const string INCOME_STATEMENT = "Income Statement";
            public const string CASH_FLOW = "Cash Flow";
            public const string TRIAL_BALANCE = "Trial Balance";
            public const string VOUCHER_SUMMARY = "Voucher Summary";
        }

        // Units of Measurement
        public static class Units
        {
            public const string PIECES = "PCS";
            public const string KILOGRAMS = "KG";
            public const string GRAMS = "G";
            public const string LITERS = "LTR";
            public const string METERS = "M";
            public const string BOX = "BOX";
            public const string PACK = "PACK";

            public static readonly string[] ALL_UNITS = { PIECES, KILOGRAMS, GRAMS, LITERS, METERS, BOX, PACK };
        }

        // Default Values
        public static class Defaults
        {
            public const decimal MIN_STOCK_LEVEL = 10;
            public const int ESTIMATE_VALIDITY_DAYS = 30;
            public const string CURRENCY_SYMBOL = "₹";
            public const string CURRENCY_CODE = "INR";
            public const string COMPANY_NAME = "My Billing Company";
            public const string COUNTRY = "India";
        }

        // Validation Limits
        public static class Limits
        {
            public const int USERNAME_MIN_LENGTH = 3;
            public const int USERNAME_MAX_LENGTH = 20;
            public const int PASSWORD_MIN_LENGTH = 6;
            public const int PRODUCT_NAME_MAX_LENGTH = 100;
            public const int PRODUCT_CODE_MAX_LENGTH = 20;
            public const int PARTY_NAME_MAX_LENGTH = 100;
            public const int DESCRIPTION_MAX_LENGTH = 500;
            public const decimal MAX_AMOUNT = 999999999.99m; // 9 digits, 2 decimals
        }

        // File Paths and Extensions
        public static class Files
        {
            public const string BACKUP_FOLDER = "Backups";
            public const string EXPORT_FOLDER = "Exports";
            public const string LOGS_FOLDER = "Logs";
            
            public const string CSV_EXTENSION = ".csv";
            public const string EXCEL_EXTENSION = ".xlsx";
            public const string PDF_EXTENSION = ".pdf";
            public const string BACKUP_EXTENSION = ".backup";
        }

        // Date Formats
        public static class DateFormats
        {
            public const string DISPLAY = "dd-MMM-yyyy";
            public const string DISPLAY_WITH_TIME = "dd-MMM-yyyy HH:mm";
            public const string DATABASE = "yyyy-MM-dd";
            public const string DATABASE_WITH_TIME = "yyyy-MM-dd HH:mm:ss";
            public const string FILENAME = "yyyyMMdd_HHmmss";
        }

        // Error Messages
        public static class ErrorMessages
        {
            public const string REQUIRED_FIELD = "This field is required.";
            public const string INVALID_EMAIL = "Please enter a valid email address.";
            public const string INVALID_PHONE = "Please enter a valid phone number.";
            public const string INVALID_NUMBER = "Please enter a valid number.";
            public const string INVALID_DATE = "Please enter a valid date.";
            public const string INVALID_DATE_RANGE = "From date cannot be after To date.";
            public const string DATABASE_ERROR = "A database error occurred. Please try again.";
            public const string NETWORK_ERROR = "Network error. Please check your connection.";
            public const string UNKNOWN_ERROR = "An unknown error occurred. Please contact support.";
        }

        // Success Messages
        public static class SuccessMessages
        {
            public const string SAVED_SUCCESS = "Saved successfully!";
            public const string DELETED_SUCCESS = "Deleted successfully!";
            public const string UPDATED_SUCCESS = "Updated successfully!";
            public const string EXPORT_SUCCESS = "Exported successfully!";
            public const string BACKUP_SUCCESS = "Backup created successfully!";
            public const string RESTORE_SUCCESS = "Data restored successfully!";
        }
    }
}