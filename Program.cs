using System;
using System.Windows.Forms;

namespace BillingSoftware
{
    internal static class Program
    {
        public static string CurrentUser { get; private set; } = "";
        public static string UserRole { get; private set; } = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // REMOVE THE BYPASS - Enable proper login
            using (var loginForm = new Forms.LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    CurrentUser = loginForm.LoggedInUsername;
                    UserRole = loginForm.UserRole;
                    Application.Run(new MainForm());
                }
            }
        }
    }
}