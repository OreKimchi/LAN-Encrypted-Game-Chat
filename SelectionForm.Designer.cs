namespace SecureChat4InARow
{
    partial class SelectionForm
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
            this.ServerButton = new System.Windows.Forms.Button();
            this.ClientButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ServerButton
            // 
            this.ServerButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(255)))), ((int)(((byte)(148)))));
            this.ServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ServerButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ServerButton.ForeColor = System.Drawing.Color.Black;
            this.ServerButton.Location = new System.Drawing.Point(12, 12);
            this.ServerButton.Name = "ServerButton";
            this.ServerButton.Size = new System.Drawing.Size(176, 65);
            this.ServerButton.TabIndex = 0;
            this.ServerButton.Text = "Server";
            this.ServerButton.UseVisualStyleBackColor = false;
            this.ServerButton.Click += new System.EventHandler(this.ServerButton_Click);
            // 
            // ClientButton
            // 
            this.ClientButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(255)))), ((int)(((byte)(148)))));
            this.ClientButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ClientButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ClientButton.Location = new System.Drawing.Point(194, 12);
            this.ClientButton.Name = "ClientButton";
            this.ClientButton.Size = new System.Drawing.Size(176, 65);
            this.ClientButton.TabIndex = 1;
            this.ClientButton.Text = "Client";
            this.ClientButton.UseVisualStyleBackColor = false;
            this.ClientButton.Click += new System.EventHandler(this.ClientButton_Click);
            // 
            // SelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(44)))), ((int)(((byte)(57)))));
            this.ClientSize = new System.Drawing.Size(382, 88);
            this.Controls.Add(this.ClientButton);
            this.Controls.Add(this.ServerButton);
            this.MaximumSize = new System.Drawing.Size(398, 127);
            this.MinimumSize = new System.Drawing.Size(398, 127);
            this.Name = "SelectionForm";
            this.Text = "Selection";
            this.Load += new System.EventHandler(this.SelectionForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ServerButton;
        private System.Windows.Forms.Button ClientButton;
    }
}

