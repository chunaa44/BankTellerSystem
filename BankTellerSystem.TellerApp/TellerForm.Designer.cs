namespace BankTellerSystem.TellerApp
{
    partial class TellerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupTicket = new GroupBox();
            lblCounter = new Label();
            numCounterId = new NumericUpDown();
            btnCallNext = new Button();
            btnComplete = new Button();
            lblCurrentTicket = new Label();
            groupTransfer = new GroupBox();
            lblFrom = new Label();
            cmbFromAccount = new ComboBox();
            lblTo = new Label();
            cmbToAccount = new ComboBox();
            lblAmount = new Label();
            txtAmount = new TextBox();
            btnTransfer = new Button();
            groupRates = new GroupBox();
            lblCurrency = new Label();
            cmbCurrency = new ComboBox();
            lblBuyRate = new Label();
            txtBuyRate = new TextBox();
            lblSellRate = new Label();
            txtSellRate = new TextBox();
            btnUpdateRate = new Button();
            txtLog = new TextBox();
            btnRefresh = new Button();
            groupTicket.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCounterId).BeginInit();
            groupTransfer.SuspendLayout();
            groupRates.SuspendLayout();
            SuspendLayout();
            // 
            // groupTicket
            // 
            groupTicket.Controls.Add(lblCounter);
            groupTicket.Controls.Add(numCounterId);
            groupTicket.Controls.Add(btnCallNext);
            groupTicket.Controls.Add(btnComplete);
            groupTicket.Controls.Add(lblCurrentTicket);
            groupTicket.Location = new Point(17, 68);
            groupTicket.Margin = new Padding(4, 5, 4, 5);
            groupTicket.Name = "groupTicket";
            groupTicket.Padding = new Padding(4, 5, 4, 5);
            groupTicket.Size = new Size(686, 167);
            groupTicket.TabIndex = 1;
            groupTicket.TabStop = false;
            groupTicket.Text = "Ticket Queue";
            // 
            // lblCounter
            // 
            lblCounter.AutoSize = true;
            lblCounter.Location = new Point(21, 50);
            lblCounter.Margin = new Padding(4, 0, 4, 0);
            lblCounter.Name = "lblCounter";
            lblCounter.Size = new Size(102, 25);
            lblCounter.TabIndex = 0;
            lblCounter.Text = "Counter ID:";
            // 
            // numCounterId
            // 
            numCounterId.Location = new Point(136, 45);
            numCounterId.Margin = new Padding(4, 5, 4, 5);
            numCounterId.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            numCounterId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCounterId.Name = "numCounterId";
            numCounterId.Size = new Size(71, 31);
            numCounterId.TabIndex = 1;
            numCounterId.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnCallNext
            // 
            btnCallNext.Location = new Point(243, 43);
            btnCallNext.Margin = new Padding(4, 5, 4, 5);
            btnCallNext.Name = "btnCallNext";
            btnCallNext.Size = new Size(157, 42);
            btnCallNext.TabIndex = 2;
            btnCallNext.Text = "Call Next";
            btnCallNext.UseVisualStyleBackColor = true;
            btnCallNext.Click += btnCallNext_Click;
            // 
            // btnComplete
            // 
            btnComplete.Location = new Point(414, 43);
            btnComplete.Margin = new Padding(4, 5, 4, 5);
            btnComplete.Name = "btnComplete";
            btnComplete.Size = new Size(214, 42);
            btnComplete.TabIndex = 3;
            btnComplete.Text = "Complete Current";
            btnComplete.UseVisualStyleBackColor = true;
            btnComplete.Click += btnComplete_Click;
            // 
            // lblCurrentTicket
            // 
            lblCurrentTicket.AutoSize = true;
            lblCurrentTicket.Location = new Point(21, 108);
            lblCurrentTicket.Margin = new Padding(4, 0, 4, 0);
            lblCurrentTicket.Name = "lblCurrentTicket";
            lblCurrentTicket.Size = new Size(133, 25);
            lblCurrentTicket.TabIndex = 4;
            lblCurrentTicket.Text = "Current ticket: -";
            // 
            // groupTransfer
            // 
            groupTransfer.Controls.Add(lblFrom);
            groupTransfer.Controls.Add(cmbFromAccount);
            groupTransfer.Controls.Add(lblTo);
            groupTransfer.Controls.Add(cmbToAccount);
            groupTransfer.Controls.Add(lblAmount);
            groupTransfer.Controls.Add(txtAmount);
            groupTransfer.Controls.Add(btnTransfer);
            groupTransfer.Location = new Point(17, 252);
            groupTransfer.Margin = new Padding(4, 5, 4, 5);
            groupTransfer.Name = "groupTransfer";
            groupTransfer.Padding = new Padding(4, 5, 4, 5);
            groupTransfer.Size = new Size(686, 233);
            groupTransfer.TabIndex = 2;
            groupTransfer.TabStop = false;
            groupTransfer.Text = "Account Transfer";
            // 
            // lblFrom
            // 
            lblFrom.AutoSize = true;
            lblFrom.Location = new Point(21, 50);
            lblFrom.Margin = new Padding(4, 0, 4, 0);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(125, 25);
            lblFrom.TabIndex = 0;
            lblFrom.Text = "From account:";
            // 
            // cmbFromAccount
            // 
            cmbFromAccount.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFromAccount.Location = new Point(171, 45);
            cmbFromAccount.Margin = new Padding(4, 5, 4, 5);
            cmbFromAccount.Name = "cmbFromAccount";
            cmbFromAccount.Size = new Size(455, 33);
            cmbFromAccount.TabIndex = 1;
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.Location = new Point(21, 103);
            lblTo.Margin = new Padding(4, 0, 4, 0);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(101, 25);
            lblTo.TabIndex = 2;
            lblTo.Text = "To account:";
            // 
            // cmbToAccount
            // 
            cmbToAccount.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbToAccount.Location = new Point(171, 98);
            cmbToAccount.Margin = new Padding(4, 5, 4, 5);
            cmbToAccount.Name = "cmbToAccount";
            cmbToAccount.Size = new Size(455, 33);
            cmbToAccount.TabIndex = 3;
            // 
            // lblAmount
            // 
            lblAmount.AutoSize = true;
            lblAmount.Location = new Point(21, 157);
            lblAmount.Margin = new Padding(4, 0, 4, 0);
            lblAmount.Name = "lblAmount";
            lblAmount.Size = new Size(81, 25);
            lblAmount.TabIndex = 4;
            lblAmount.Text = "Amount:";
            // 
            // txtAmount
            // 
            txtAmount.Location = new Point(171, 152);
            txtAmount.Margin = new Padding(4, 5, 4, 5);
            txtAmount.Name = "txtAmount";
            txtAmount.Size = new Size(213, 31);
            txtAmount.TabIndex = 5;
            // 
            // btnTransfer
            // 
            btnTransfer.Location = new Point(414, 150);
            btnTransfer.Margin = new Padding(4, 5, 4, 5);
            btnTransfer.Name = "btnTransfer";
            btnTransfer.Size = new Size(214, 42);
            btnTransfer.TabIndex = 6;
            btnTransfer.Text = "Transfer";
            btnTransfer.UseVisualStyleBackColor = true;
            btnTransfer.Click += btnTransfer_Click;
            // 
            // groupRates
            // 
            groupRates.Controls.Add(lblCurrency);
            groupRates.Controls.Add(cmbCurrency);
            groupRates.Controls.Add(lblBuyRate);
            groupRates.Controls.Add(txtBuyRate);
            groupRates.Controls.Add(lblSellRate);
            groupRates.Controls.Add(txtSellRate);
            groupRates.Controls.Add(btnUpdateRate);
            groupRates.Location = new Point(17, 502);
            groupRates.Margin = new Padding(4, 5, 4, 5);
            groupRates.Name = "groupRates";
            groupRates.Padding = new Padding(4, 5, 4, 5);
            groupRates.Size = new Size(686, 233);
            groupRates.TabIndex = 3;
            groupRates.TabStop = false;
            groupRates.Text = "Exchange Rates";
            // 
            // lblCurrency
            // 
            lblCurrency.AutoSize = true;
            lblCurrency.Location = new Point(21, 50);
            lblCurrency.Margin = new Padding(4, 0, 4, 0);
            lblCurrency.Name = "lblCurrency";
            lblCurrency.Size = new Size(85, 25);
            lblCurrency.TabIndex = 0;
            lblCurrency.Text = "Currency:";
            // 
            // cmbCurrency
            // 
            cmbCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCurrency.Location = new Point(171, 45);
            cmbCurrency.Margin = new Padding(4, 5, 4, 5);
            cmbCurrency.Name = "cmbCurrency";
            cmbCurrency.Size = new Size(275, 33);
            cmbCurrency.TabIndex = 1;
            cmbCurrency.SelectedIndexChanged += cmbCurrency_SelectedIndexChanged;
            // 
            // lblBuyRate
            // 
            lblBuyRate.AutoSize = true;
            lblBuyRate.Location = new Point(21, 103);
            lblBuyRate.Margin = new Padding(4, 0, 4, 0);
            lblBuyRate.Name = "lblBuyRate";
            lblBuyRate.Size = new Size(80, 25);
            lblBuyRate.TabIndex = 2;
            lblBuyRate.Text = "Buy rate:";
            // 
            // txtBuyRate
            // 
            txtBuyRate.Location = new Point(171, 98);
            txtBuyRate.Margin = new Padding(4, 5, 4, 5);
            txtBuyRate.Name = "txtBuyRate";
            txtBuyRate.Size = new Size(213, 31);
            txtBuyRate.TabIndex = 3;
            // 
            // lblSellRate
            // 
            lblSellRate.AutoSize = true;
            lblSellRate.Location = new Point(21, 157);
            lblSellRate.Margin = new Padding(4, 0, 4, 0);
            lblSellRate.Name = "lblSellRate";
            lblSellRate.Size = new Size(78, 25);
            lblSellRate.TabIndex = 4;
            lblSellRate.Text = "Sell rate:";
            // 
            // txtSellRate
            // 
            txtSellRate.Location = new Point(171, 152);
            txtSellRate.Margin = new Padding(4, 5, 4, 5);
            txtSellRate.Name = "txtSellRate";
            txtSellRate.Size = new Size(213, 31);
            txtSellRate.TabIndex = 5;
            // 
            // btnUpdateRate
            // 
            btnUpdateRate.Location = new Point(414, 150);
            btnUpdateRate.Margin = new Padding(4, 5, 4, 5);
            btnUpdateRate.Name = "btnUpdateRate";
            btnUpdateRate.Size = new Size(214, 42);
            btnUpdateRate.TabIndex = 6;
            btnUpdateRate.Text = "Update Rate";
            btnUpdateRate.UseVisualStyleBackColor = true;
            btnUpdateRate.Click += btnUpdateRate_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(17, 752);
            txtLog.Margin = new Padding(4, 5, 4, 5);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(684, 197);
            txtLog.TabIndex = 4;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(17, 20);
            btnRefresh.Margin = new Padding(4, 5, 4, 5);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(214, 42);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "Refresh Data";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // TellerForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 972);
            Controls.Add(btnRefresh);
            Controls.Add(groupTicket);
            Controls.Add(groupTransfer);
            Controls.Add(groupRates);
            Controls.Add(txtLog);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TellerForm";
            Text = "Teller App";
            groupTicket.ResumeLayout(false);
            groupTicket.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCounterId).EndInit();
            groupTransfer.ResumeLayout(false);
            groupTransfer.PerformLayout();
            groupRates.ResumeLayout(false);
            groupRates.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private GroupBox groupTicket;
        private Label lblCounter;
        private NumericUpDown numCounterId;
        private Button btnCallNext;
        private Button btnComplete;
        private Label lblCurrentTicket;
        private GroupBox groupTransfer;
        private Label lblFrom;
        private ComboBox cmbFromAccount;
        private Label lblTo;
        private ComboBox cmbToAccount;
        private Label lblAmount;
        private TextBox txtAmount;
        private Button btnTransfer;
        private GroupBox groupRates;
        private Label lblCurrency;
        private ComboBox cmbCurrency;
        private Label lblBuyRate;
        private TextBox txtBuyRate;
        private Label lblSellRate;
        private TextBox txtSellRate;
        private Button btnUpdateRate;
        private TextBox txtLog;
        private Button btnRefresh;
    }
        #endregion

}