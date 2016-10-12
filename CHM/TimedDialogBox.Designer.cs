namespace CHM
{
    partial class TimedDialogBox
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
            this.TimedDialogBoxStuff = new System.Windows.Forms.Label();
            this.TDB_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TimedDialogBoxStuff
            // 
            this.TimedDialogBoxStuff.AutoSize = true;
            this.TimedDialogBoxStuff.Location = new System.Drawing.Point(12, 3);
            this.TimedDialogBoxStuff.Name = "TimedDialogBoxStuff";
            this.TimedDialogBoxStuff.Size = new System.Drawing.Size(35, 13);
            this.TimedDialogBoxStuff.TabIndex = 1;
            this.TimedDialogBoxStuff.Text = "label1";
            // 
            // TDB_button
            // 
            this.TDB_button.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.TDB_button.Location = new System.Drawing.Point(119, 64);
            this.TDB_button.Name = "TDB_button";
            this.TDB_button.Size = new System.Drawing.Size(81, 32);
            this.TDB_button.TabIndex = 2;
            this.TDB_button.Text = "Done";
            this.TDB_button.UseVisualStyleBackColor = true;
            this.TDB_button.Click += new System.EventHandler(this.TDB_button_Click);
            // 
            // TimedDialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(346, 106);
            this.Controls.Add(this.TDB_button);
            this.Controls.Add(this.TimedDialogBoxStuff);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimedDialogBox";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TimedDialogBox";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.TimedDialogBox_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TimedDialogBoxStuff;
        private System.Windows.Forms.Button TDB_button;
    }
}