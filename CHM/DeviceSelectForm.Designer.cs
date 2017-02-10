namespace CHM
{
    partial class DeviceSelectForm
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
            this.Cancel = new System.Windows.Forms.Button();
            this.Display = new System.Windows.Forms.Button();
            this.All = new System.Windows.Forms.Button();
            this.SelectionBox = new System.Windows.Forms.GroupBox();
            this.RBSetable = new System.Windows.Forms.RadioButton();
            this.RBSensors = new System.Windows.Forms.RadioButton();
            this.RBDevices = new System.Windows.Forms.RadioButton();
            this.RBAll = new System.Windows.Forms.RadioButton();
            this.SelectionBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(175, 5);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 0;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Display
            // 
            this.Display.Location = new System.Drawing.Point(12, 5);
            this.Display.Name = "Display";
            this.Display.Size = new System.Drawing.Size(76, 23);
            this.Display.TabIndex = 1;
            this.Display.Text = "Display";
            this.Display.UseVisualStyleBackColor = true;
            // 
            // All
            // 
            this.All.Location = new System.Drawing.Point(94, 5);
            this.All.Name = "All";
            this.All.Size = new System.Drawing.Size(75, 23);
            this.All.TabIndex = 2;
            this.All.Text = "All";
            this.All.UseVisualStyleBackColor = true;
            // 
            // SelectionBox
            // 
            this.SelectionBox.Controls.Add(this.RBSetable);
            this.SelectionBox.Controls.Add(this.RBSensors);
            this.SelectionBox.Controls.Add(this.RBDevices);
            this.SelectionBox.Controls.Add(this.RBAll);
            this.SelectionBox.Location = new System.Drawing.Point(251, -2);
            this.SelectionBox.Name = "SelectionBox";
            this.SelectionBox.Size = new System.Drawing.Size(254, 29);
            this.SelectionBox.TabIndex = 6;
            this.SelectionBox.TabStop = false;
            // 
            // RBSetable
            // 
            this.RBSetable.AutoSize = true;
            this.RBSetable.Checked = true;
            this.RBSetable.Location = new System.Drawing.Point(186, 12);
            this.RBSetable.Name = "RBSetable";
            this.RBSetable.Size = new System.Drawing.Size(61, 17);
            this.RBSetable.TabIndex = 9;
            this.RBSetable.TabStop = true;
            this.RBSetable.Text = "Setable";
            this.RBSetable.UseVisualStyleBackColor = true;
            // 
            // RBSensors
            // 
            this.RBSensors.AutoSize = true;
            this.RBSensors.Location = new System.Drawing.Point(117, 12);
            this.RBSensors.Name = "RBSensors";
            this.RBSensors.Size = new System.Drawing.Size(63, 17);
            this.RBSensors.TabIndex = 8;
            this.RBSensors.Text = "Sensors";
            this.RBSensors.UseVisualStyleBackColor = true;
            // 
            // RBDevices
            // 
            this.RBDevices.AutoSize = true;
            this.RBDevices.Location = new System.Drawing.Point(50, 12);
            this.RBDevices.Name = "RBDevices";
            this.RBDevices.Size = new System.Drawing.Size(64, 17);
            this.RBDevices.TabIndex = 7;
            this.RBDevices.Text = "Devices";
            this.RBDevices.UseVisualStyleBackColor = true;
            // 
            // RBAll
            // 
            this.RBAll.AutoSize = true;
            this.RBAll.Location = new System.Drawing.Point(8, 12);
            this.RBAll.Name = "RBAll";
            this.RBAll.Size = new System.Drawing.Size(36, 17);
            this.RBAll.TabIndex = 6;
            this.RBAll.Text = "All";
            this.RBAll.UseVisualStyleBackColor = true;
            // 
            // DeviceSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(511, 35);
            this.Controls.Add(this.SelectionBox);
            this.Controls.Add(this.All);
            this.Controls.Add(this.Display);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DeviceSelectForm";
            this.Text = "DeviceSelectForm";
            this.SelectionBox.ResumeLayout(false);
            this.SelectionBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Display;
        private System.Windows.Forms.Button All;
        private System.Windows.Forms.RadioButton RBSensors;
        private System.Windows.Forms.RadioButton RBDevices;
        private System.Windows.Forms.RadioButton RBAll;
        internal System.Windows.Forms.GroupBox SelectionBox;
        private System.Windows.Forms.RadioButton RBSetable;
    }
}