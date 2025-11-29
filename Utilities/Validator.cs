using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BillingSoftware.Utilities
{
    public static class Validator
    {
        /// <summary>
        /// Validates if a string is not null, empty, or whitespace
        /// </summary>
        public static bool IsRequired(string value, string fieldName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = $"{fieldName} is required.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a string has minimum length
        /// </summary>
        public static bool HasMinLength(string value, int minLength, string fieldName, out string errorMessage)
        {
            if (value?.Length < minLength)
            {
                errorMessage = $"{fieldName} must be at least {minLength} characters long.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a string has maximum length
        /// </summary>
        public static bool HasMaxLength(string value, int maxLength, string fieldName, out string errorMessage)
        {
            if (value?.Length > maxLength)
            {
                errorMessage = $"{fieldName} cannot exceed {maxLength} characters.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a string is a valid decimal number
        /// </summary>
        public static bool IsDecimal(string value, string fieldName, out string errorMessage)
        {
            if (!decimal.TryParse(value, out _))
            {
                errorMessage = $"{fieldName} must be a valid number.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a decimal value is positive
        /// </summary>
        public static bool IsPositive(decimal value, string fieldName, out string errorMessage)
        {
            if (value <= 0)
            {
                errorMessage = $"{fieldName} must be greater than zero.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a string is a valid integer
        /// </summary>
        public static bool IsInteger(string value, string fieldName, out string errorMessage)
        {
            if (!int.TryParse(value, out _))
            {
                errorMessage = $"{fieldName} must be a whole number.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a string is a valid email address
        /// </summary>
        public static bool IsEmail(string value, string fieldName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = null; // Email is optional
                return true;
            }

            // Simple email validation regex
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(value, pattern))
            {
                errorMessage = $"{fieldName} is not a valid email address.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a string is a valid phone number
        /// </summary>
        public static bool IsPhoneNumber(string value, string fieldName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = null; // Phone is optional
                return true;
            }

            // Remove common separators and check if it's all digits
            string cleanNumber = new string(value.Where(char.IsDigit).ToArray());
            
            if (cleanNumber.Length < 10 || cleanNumber.Length > 15)
            {
                errorMessage = $"{fieldName} must be between 10 and 15 digits.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a string is a valid GST number (basic validation)
        /// </summary>
        public static bool IsGSTNumber(string value, string fieldName, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorMessage = null; // GST is optional
                return true;
            }

            // Basic GST validation - 15 characters alphanumeric
            if (value.Length != 15 || !value.All(c => char.IsLetterOrDigit(c)))
            {
                errorMessage = $"{fieldName} must be 15 characters alphanumeric.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a date is not in the future
        /// </summary>
        public static bool IsNotFutureDate(DateTime date, string fieldName, out string errorMessage)
        {
            if (date > DateTime.Now)
            {
                errorMessage = $"{fieldName} cannot be in the future.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates if a date range is valid (fromDate <= toDate)
        /// </summary>
        public static bool IsValidDateRange(DateTime fromDate, DateTime toDate, out string errorMessage)
        {
            if (fromDate > toDate)
            {
                errorMessage = "From date cannot be after To date.";
                return false;
            }
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Validates username (alphanumeric, 3-20 characters)
        /// </summary>
        public static bool IsValidUsername(string username, out string errorMessage)
        {
            if (!IsRequired(username, "Username", out errorMessage)) return false;
            if (!HasMinLength(username, 3, "Username", out errorMessage)) return false;
            if (!HasMaxLength(username, 20, "Username", out errorMessage)) return false;

            if (!username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            {
                errorMessage = "Username can only contain letters, numbers, underscores and hyphens.";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates password (minimum 6 characters)
        /// </summary>
        public static bool IsValidPassword(string password, out string errorMessage)
        {
            if (!IsRequired(password, "Password", out errorMessage)) return false;
            if (!HasMinLength(password, 6, "Password", out errorMessage)) return false;
            return true;
        }

        /// <summary>
        /// Sets error on control and shows error message
        /// </summary>
        public static void SetError(Control control, string errorMessage)
        {
            if (control is TextBox textBox)
            {
                textBox.BackColor = Color.LightPink;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            
            // You could use ErrorProvider here for more professional look
            // For now, we'll just show a message box
            if (!string.IsNullOrEmpty(errorMessage))
            {
                UIHelper.ShowError(errorMessage);
            }
        }

        /// <summary>
        /// Clears error from control
        /// </summary>
        public static void ClearError(Control control)
        {
            if (control is TextBox textBox)
            {
                textBox.BackColor = UIHelper.White;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        /// <summary>
        /// Validates all controls in a container
        /// </summary>
        public static bool ValidateForm(Control container)
        {
            foreach (Control control in container.Controls)
            {
                if (control is TextBox textBox && textBox.Visible && textBox.Enabled)
                {
                    // Add specific validation logic based on control name or tag
                    if (textBox.Tag?.ToString() == "required" && string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        SetError(textBox, "This field is required.");
                        return false;
                    }
                }
                
                // Recursively validate child controls
                if (control.HasChildren)
                {
                    if (!ValidateForm(control))
                        return false;
                }
            }
            return true;
        }
    }
}