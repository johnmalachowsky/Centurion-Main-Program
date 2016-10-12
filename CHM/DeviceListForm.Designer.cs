namespace CHM
{
    partial class DeviceListForm
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
            this.components = new System.ComponentModel.Container();
            this.DeviceListFormTimer = new System.Windows.Forms.Timer(this.components);
            this.Close = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // DeviceListFormTimer
            // 
            this.DeviceListFormTimer.Enabled = true;
            // 
            // Close
            // 
            this.Close.Location = new System.Drawing.Point(12, 21);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(75, 23);
            this.Close.TabIndex = 0;
            this.Close.Text = "Close";
            this.Close.UseVisualStyleBackColor = true;
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(146, 20);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 1;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            // 
            // DeviceListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(367, 56);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Close);
            this.Name = "DeviceListForm";
            this.Text = "DeviceListForm";
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Timer DeviceListFormTimer;
        private System.Windows.Forms.Button Close;
        private System.Windows.Forms.Button Save;
    }
}