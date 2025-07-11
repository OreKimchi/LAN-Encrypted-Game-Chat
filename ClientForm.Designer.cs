namespace SecureChat4InARow
{
    partial class ClientForm
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
            this.ClientLog = new System.Windows.Forms.RichTextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.chatInput = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.sendInstructionsLabel = new System.Windows.Forms.Label();
            this.ChallengeButton = new System.Windows.Forms.Button();
            this.opponentTextBox = new System.Windows.Forms.TextBox();
            this.challengeInstructionsLabel = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // ClientLog
            // 
            this.ClientLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(88)))), ((int)(((byte)(110)))));
            this.ClientLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ClientLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ClientLog.Location = new System.Drawing.Point(12, 228);
            this.ClientLog.Name = "ClientLog";
            this.ClientLog.Size = new System.Drawing.Size(641, 218);
            this.ClientLog.TabIndex = 0;
            this.ClientLog.Text = "";
            // 
            // SendButton
            // 
            this.SendButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(255)))), ((int)(((byte)(148)))));
            this.SendButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SendButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.SendButton.Location = new System.Drawing.Point(241, 89);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(119, 45);
            this.SendButton.TabIndex = 1;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = false;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // chatInput
            // 
            this.chatInput.Location = new System.Drawing.Point(26, 89);
            this.chatInput.Multiline = true;
            this.chatInput.Name = "chatInput";
            this.chatInput.Size = new System.Drawing.Size(192, 45);
            this.chatInput.TabIndex = 2;
            this.chatInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChatInput_KeyDown);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SecureChat4InARow.Properties.Resources.ClientIcon;
            this.pictureBox1.Location = new System.Drawing.Point(362, 31);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(291, 172);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // sendInstructionsLabel
            // 
            this.sendInstructionsLabel.AutoSize = true;
            this.sendInstructionsLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(88)))), ((int)(((byte)(110)))));
            this.sendInstructionsLabel.ForeColor = System.Drawing.Color.White;
            this.sendInstructionsLabel.Location = new System.Drawing.Point(23, 64);
            this.sendInstructionsLabel.Name = "sendInstructionsLabel";
            this.sendInstructionsLabel.Size = new System.Drawing.Size(210, 13);
            this.sendInstructionsLabel.TabIndex = 4;
            this.sendInstructionsLabel.Text = "Click Send or Press Enter to send message";
            // 
            // ChallengeButton
            // 
            this.ChallengeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(255)))), ((int)(((byte)(148)))));
            this.ChallengeButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ChallengeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.ChallengeButton.Location = new System.Drawing.Point(241, 158);
            this.ChallengeButton.Name = "ChallengeButton";
            this.ChallengeButton.Size = new System.Drawing.Size(130, 45);
            this.ChallengeButton.TabIndex = 5;
            this.ChallengeButton.Text = "Challenge";
            this.ChallengeButton.UseVisualStyleBackColor = false;
            this.ChallengeButton.Click += new System.EventHandler(this.ChallengeButton_Click);
            // 
            // opponentTextBox
            // 
            this.opponentTextBox.Location = new System.Drawing.Point(26, 174);
            this.opponentTextBox.Name = "opponentTextBox";
            this.opponentTextBox.Size = new System.Drawing.Size(192, 20);
            this.opponentTextBox.TabIndex = 6;
            // 
            // challengeInstructionsLabel
            // 
            this.challengeInstructionsLabel.AutoSize = true;
            this.challengeInstructionsLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(88)))), ((int)(((byte)(110)))));
            this.challengeInstructionsLabel.ForeColor = System.Drawing.Color.White;
            this.challengeInstructionsLabel.Location = new System.Drawing.Point(23, 149);
            this.challengeInstructionsLabel.Name = "challengeInstructionsLabel";
            this.challengeInstructionsLabel.Size = new System.Drawing.Size(141, 13);
            this.challengeInstructionsLabel.TabIndex = 7;
            this.challengeInstructionsLabel.Text = "Type username to challenge";
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(88)))), ((int)(((byte)(110)))));
            this.usernameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.usernameLabel.ForeColor = System.Drawing.Color.White;
            this.usernameLabel.Location = new System.Drawing.Point(237, 9);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(0, 24);
            this.usernameLabel.TabIndex = 8;
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(44)))), ((int)(((byte)(57)))));
            this.ClientSize = new System.Drawing.Size(665, 450);
            this.Controls.Add(this.usernameLabel);
            this.Controls.Add(this.challengeInstructionsLabel);
            this.Controls.Add(this.opponentTextBox);
            this.Controls.Add(this.ChallengeButton);
            this.Controls.Add(this.sendInstructionsLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.chatInput);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.ClientLog);
            this.MaximumSize = new System.Drawing.Size(681, 489);
            this.MinimumSize = new System.Drawing.Size(681, 489);
            this.Name = "ClientForm";
            this.Text = "ClientForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_Closing);
            this.Load += new System.EventHandler(this.ClientForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox ClientLog;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.TextBox chatInput;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label sendInstructionsLabel;
        private System.Windows.Forms.Button ChallengeButton;
        private System.Windows.Forms.TextBox opponentTextBox;
        private System.Windows.Forms.Label challengeInstructionsLabel;
        private System.Windows.Forms.Label usernameLabel;
    }
}