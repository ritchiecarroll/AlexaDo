namespace AlexaDo
{
    partial class TestCommand
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
            this.CommandText = new System.Windows.Forms.TextBox();
            this.CancelTestButton = new System.Windows.Forms.Button();
            this.ExecuteTestButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CommandText
            // 
            this.CommandText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandText.Location = new System.Drawing.Point(13, 13);
            this.CommandText.Multiline = true;
            this.CommandText.Name = "CommandText";
            this.CommandText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CommandText.Size = new System.Drawing.Size(372, 67);
            this.CommandText.TabIndex = 0;
            this.CommandText.Text = "Simon Says enter command text to test";
            // 
            // CancelTestButton
            // 
            this.CancelTestButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelTestButton.Location = new System.Drawing.Point(310, 86);
            this.CancelTestButton.Name = "CancelTestButton";
            this.CancelTestButton.Size = new System.Drawing.Size(75, 23);
            this.CancelTestButton.TabIndex = 2;
            this.CancelTestButton.Text = "&Cancel";
            this.CancelTestButton.UseVisualStyleBackColor = true;
            // 
            // ExecuteTestButton
            // 
            this.ExecuteTestButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ExecuteTestButton.Location = new System.Drawing.Point(228, 86);
            this.ExecuteTestButton.Name = "ExecuteTestButton";
            this.ExecuteTestButton.Size = new System.Drawing.Size(75, 23);
            this.ExecuteTestButton.TabIndex = 1;
            this.ExecuteTestButton.Text = "&Test";
            this.ExecuteTestButton.UseVisualStyleBackColor = true;
            this.ExecuteTestButton.Click += new System.EventHandler(this.ExecuteTestButton_Click);
            // 
            // TestCommand
            // 
            this.AcceptButton = this.ExecuteTestButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelTestButton;
            this.ClientSize = new System.Drawing.Size(397, 116);
            this.ControlBox = false;
            this.Controls.Add(this.ExecuteTestButton);
            this.Controls.Add(this.CancelTestButton);
            this.Controls.Add(this.CommandText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TestCommand";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Test Command";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CancelTestButton;
        private System.Windows.Forms.Button ExecuteTestButton;
        internal System.Windows.Forms.TextBox CommandText;
    }
}