namespace CHM
{
    partial class TimedDialogBoxWithListbox
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
            this.TDBWL_Button = new System.Windows.Forms.Button();
            this.TDBWL_listBox = new System.Windows.Forms.ListBox();
            this.TDBWL_Button2 = new System.Windows.Forms.Button();
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
            // TDBWL_Button
            // 
            this.TDBWL_Button.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.TDBWL_Button.Location = new System.Drawing.Point(41, 214);
            this.TDBWL_Button.Name = "TDBWL_Button";
            this.TDBWL_Button.Size = new System.Drawing.Size(81, 43);
            this.TDBWL_Button.TabIndex = 2;
            this.TDBWL_Button.Text = "Done";
            this.TDBWL_Button.UseVisualStyleBackColor = true;
            this.TDBWL_Button.Click += new System.EventHandler(this.TDBWL_Button_Click);
            // 
            // TDBWL_listBox
            // 
            this.TDBWL_listBox.FormattingEnabled = true;
            this.TDBWL_listBox.Location = new System.Drawing.Point(12, 63);
            this.TDBWL_listBox.Name = "TDBWL_listBox";
            this.TDBWL_listBox.ScrollAlwaysVisible = true;
            this.TDBWL_listBox.Size = new System.Drawing.Size(322, 134);
            this.TDBWL_listBox.TabIndex = 3;
            // 
            // TDBWL_Button2
            // 
            this.TDBWL_Button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.TDBWL_Button2.Location = new System.Drawing.Point(166, 214);
            this.TDBWL_Button2.Name = "TDBWL_Button2";
            this.TDBWL_Button2.Size = new System.Drawing.Size(81, 43);
            this.TDBWL_Button2.TabIndex = 4;
            this.TDBWL_Button2.Text = "Save Listbox As Log File";
            this.TDBWL_Button2.UseVisualStyleBackColor = true;
            this.TDBWL_Button2.Visible = false;
            this.TDBWL_Button2.Click += new System.EventHandler(this.TDBWL_Button2_Click);
            // 
            // TimedDialogBoxWithListbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(346, 261);
            this.Controls.Add(this.TDBWL_Button2);
            this.Controls.Add(this.TDBWL_listBox);
            this.Controls.Add(this.TDBWL_Button);
            this.Controls.Add(this.TimedDialogBoxStuff);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TimedDialogBoxWithListbox";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TimedDialogBoxWithListbox";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.TimedDialogBoxWithListbox_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TimedDialogBoxStuff;
        private System.Windows.Forms.Button TDBWL_Button;
        private System.Windows.Forms.ListBox TDBWL_listBox;
        private System.Windows.Forms.Button TDBWL_Button2;
    }
}