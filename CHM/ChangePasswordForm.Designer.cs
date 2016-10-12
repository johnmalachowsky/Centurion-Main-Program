namespace CHM
{
    partial class ChangePasswordForm
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
            this.OldPassword = new System.Windows.Forms.TextBox();
            this.NewPassword = new System.Windows.Forms.TextBox();
            this.PasswordStrength = new System.Windows.Forms.Label();
            this.Requirements = new System.Windows.Forms.Label();
            this.ReenterPassword = new System.Windows.Forms.TextBox();
            this.DuplicateOK = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Save = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OldPassword
            // 
            this.OldPassword.Location = new System.Drawing.Point(132, 22);
            this.OldPassword.Name = "OldPassword";
            this.OldPassword.PasswordChar = '*';
            this.OldPassword.Size = new System.Drawing.Size(228, 20);
            this.OldPassword.TabIndex = 0;
            this.OldPassword.UseSystemPasswordChar = true;
            // 
            // NewPassword
            // 
            this.NewPassword.Location = new System.Drawing.Point(132, 70);
            this.NewPassword.Name = "NewPassword";
            this.NewPassword.PasswordChar = '*';
            this.NewPassword.Size = new System.Drawing.Size(228, 20);
            this.NewPassword.TabIndex = 1;
            this.NewPassword.UseSystemPasswordChar = true;
            this.NewPassword.TextChanged += new System.EventHandler(this.NewPassword_TextChanged);
            // 
            // PasswordStrength
            // 
            this.PasswordStrength.AutoSize = true;
            this.PasswordStrength.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PasswordStrength.Location = new System.Drawing.Point(377, 68);
            this.PasswordStrength.Name = "PasswordStrength";
            this.PasswordStrength.Size = new System.Drawing.Size(109, 13);
            this.PasswordStrength.TabIndex = 2;
            this.PasswordStrength.Text = "PasswordStrength";
            // 
            // Requirements
            // 
            this.Requirements.AutoSize = true;
            this.Requirements.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Requirements.Location = new System.Drawing.Point(378, 83);
            this.Requirements.Name = "Requirements";
            this.Requirements.Size = new System.Drawing.Size(84, 13);
            this.Requirements.TabIndex = 3;
            this.Requirements.Text = "Requirements";
            // 
            // ReenterPassword
            // 
            this.ReenterPassword.Location = new System.Drawing.Point(132, 113);
            this.ReenterPassword.Name = "ReenterPassword";
            this.ReenterPassword.PasswordChar = '*';
            this.ReenterPassword.Size = new System.Drawing.Size(228, 20);
            this.ReenterPassword.TabIndex = 4;
            this.ReenterPassword.UseSystemPasswordChar = true;
            this.ReenterPassword.TextChanged += new System.EventHandler(this.ReenterPassword_TextChanged);
            // 
            // DuplicateOK
            // 
            this.DuplicateOK.AutoSize = true;
            this.DuplicateOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DuplicateOK.Location = new System.Drawing.Point(378, 116);
            this.DuplicateOK.Name = "DuplicateOK";
            this.DuplicateOK.Size = new System.Drawing.Size(41, 13);
            this.DuplicateOK.TabIndex = 5;
            this.DuplicateOK.Text = "label1";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "label2";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "label3";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(41, 156);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(118, 23);
            this.Save.TabIndex = 8;
            this.Save.Text = "Change Password";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(282, 156);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(118, 23);
            this.Cancel.TabIndex = 9;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // ChangePasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 188);
            this.ControlBox = false;
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DuplicateOK);
            this.Controls.Add(this.ReenterPassword);
            this.Controls.Add(this.Requirements);
            this.Controls.Add(this.PasswordStrength);
            this.Controls.Add(this.NewPassword);
            this.Controls.Add(this.OldPassword);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangePasswordForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change Password";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.ChangePasswordForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox OldPassword;
        private System.Windows.Forms.TextBox NewPassword;
        private System.Windows.Forms.Label PasswordStrength;
        private System.Windows.Forms.Label Requirements;
        private System.Windows.Forms.TextBox ReenterPassword;
        private System.Windows.Forms.Label DuplicateOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button Cancel;
    }
}