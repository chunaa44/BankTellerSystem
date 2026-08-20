namespace BankTellerSystem.DispenserApp
{
    partial class DispenserForm
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
            _takeNumberButton = new Button();
            _resultLabel = new Label();
            SuspendLayout();
            // 
            // _takeNumberButton
            // 
            _takeNumberButton.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _takeNumberButton.Location = new Point(132, 77);
            _takeNumberButton.Name = "_takeNumberButton";
            _takeNumberButton.Size = new Size(247, 60);
            _takeNumberButton.TabIndex = 0;
            _takeNumberButton.Text = "Take a Number";
            _takeNumberButton.UseVisualStyleBackColor = true;
            _takeNumberButton.Click += _takeNumberButton_Click;
            // 
            // _resultLabel
            // 
            _resultLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            _resultLabel.Location = new Point(21, 140);
            _resultLabel.Name = "_resultLabel";
            _resultLabel.Size = new Size(464, 134);
            _resultLabel.TabIndex = 1;
            _resultLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // DispenserForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(508, 300);
            Controls.Add(_resultLabel);
            Controls.Add(_takeNumberButton);
            Name = "DispenserForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DispenserForm";
            ResumeLayout(false);
        }

        #endregion

        private Button _takeNumberButton;
        private Label _resultLabel;
    }
}