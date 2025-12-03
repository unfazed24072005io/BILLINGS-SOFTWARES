using System;
using System.Drawing;
using System.Windows.Forms;
using BillingSoftware.Modules;
using BillingSoftware.Utilities;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class PaymentForm : Form
    {
        public PaymentForm()
        {
            InitializeComponent();
            CreatePaymentFormUI();
        }
        
        private void CreatePaymentFormUI()
        {
            this.Text = "Payment Voucher";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = TallyUIStyles.TallyGray;
            
            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "💳 PAYMENT VOUCHER";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = TallyUIStyles.TallyBlue;
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);
            
            // Simple form for now - will be implemented similar to ReceiptForm
            Label comingSoonLabel = new Label();
            comingSoonLabel.Text = "Payment Voucher Form\n(Coming Soon)";
            comingSoonLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            comingSoonLabel.ForeColor = TallyUIStyles.TallyOrange;
            comingSoonLabel.Location = new Point(250, 200);
            comingSoonLabel.Size = new Size(300, 100);
            comingSoonLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(comingSoonLabel);
            
            // Close button
            Button closeBtn = TallyUIStyles.CreateTallyButton("Close", TallyUIStyles.TallyGray, new Point(350, 350));
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(300, 250);
            this.Name = "PaymentForm";
            this.ResumeLayout(false);
        }
    }
}