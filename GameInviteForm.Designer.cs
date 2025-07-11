namespace SecureChat4InARow
{
    partial class GameInviteForm
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
            this.AcceptButton = new System.Windows.Forms.Button();
            this.DeclineButton = new System.Windows.Forms.Button();
            this.labelMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // AcceptButton
            // 
            this.AcceptButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(255)))), ((int)(((byte)(148)))));
            this.AcceptButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AcceptButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.AcceptButton.Location = new System.Drawing.Point(12, 43);
            this.AcceptButton.Name = "AcceptButton";
            this.AcceptButton.Size = new System.Drawing.Size(176, 65);
            this.AcceptButton.TabIndex = 0;
            this.AcceptButton.Text = "Accept";
            this.AcceptButton.UseVisualStyleBackColor = false;
            this.AcceptButton.Click += new System.EventHandler(this.AcceptButton_Click);
            // 
            // DeclineButton
            // 
            this.DeclineButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(255)))), ((int)(((byte)(148)))));
            this.DeclineButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.DeclineButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.DeclineButton.Location = new System.Drawing.Point(194, 43);
            this.DeclineButton.Name = "DeclineButton";
            this.DeclineButton.Size = new System.Drawing.Size(176, 65);
            this.DeclineButton.TabIndex = 1;
            this.DeclineButton.Text = "Decline";
            this.DeclineButton.UseVisualStyleBackColor = false;
            this.DeclineButton.Click += new System.EventHandler(this.DeclineButton_Click);
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(88)))), ((int)(((byte)(110)))));
            this.labelMessage.ForeColor = System.Drawing.Color.White;
            this.labelMessage.Location = new System.Drawing.Point(12, 18);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(244, 13);
            this.labelMessage.TabIndex = 2;
            this.labelMessage.Text = "User [{requesterUsername}] invites you to a game.";
            // 
            // GameInviteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(44)))), ((int)(((byte)(57)))));
            this.ClientSize = new System.Drawing.Size(382, 120);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.DeclineButton);
            this.Controls.Add(this.AcceptButton);
            this.MaximumSize = new System.Drawing.Size(398, 159);
            this.MinimumSize = new System.Drawing.Size(398, 159);
            this.Name = "GameInviteForm";
            this.Text = "GameInviteForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameInviteForm_Closing);
            this.Load += new System.EventHandler(this.GameInviteForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button AcceptButton;
        private System.Windows.Forms.Button DeclineButton;
        private System.Windows.Forms.Label labelMessage;
    }
}