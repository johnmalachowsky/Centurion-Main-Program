namespace CHM
{
    partial class MainCHMForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainCHMForm));
            this.CHMMainFormMessageBox = new System.Windows.Forms.ListBox();
            this.Exit = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.PluginStatus = new System.Windows.Forms.ListBox();
            this.TimeSliceStatus = new System.Windows.Forms.ListBox();
            this.ElapsedTime = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.FlagBox = new System.Windows.Forms.ListBox();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.debugToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.debugOnToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.debugOffToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDebugVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugList = new System.Windows.Forms.ToolStripComboBox();
            this.ToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeDatabasePasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitAndDecryptDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImmediateCommands = new System.Windows.Forms.TextBox();
            this.Execute = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.ImmediateCommandsResponse = new System.Windows.Forms.ListBox();
            this.ImmediateCommandResponsePanel = new System.Windows.Forms.Panel();
            this.FullDebug = new System.Windows.Forms.CheckBox();
            this.TestOnly = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.VoiceBox = new System.Windows.Forms.ListBox();
            this.ActionItemsListBox = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.UserInterfaceButton = new System.Windows.Forms.Button();
            this.MainMenu.SuspendLayout();
            this.ImmediateCommandResponsePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // CHMMainFormMessageBox
            // 
            this.CHMMainFormMessageBox.FormattingEnabled = true;
            this.CHMMainFormMessageBox.Location = new System.Drawing.Point(12, 201);
            this.CHMMainFormMessageBox.Name = "CHMMainFormMessageBox";
            this.CHMMainFormMessageBox.Size = new System.Drawing.Size(1036, 121);
            this.CHMMainFormMessageBox.TabIndex = 0;
            this.CHMMainFormMessageBox.DoubleClick += new System.EventHandler(this.CHMMainFormMessageBox_DoubleClick);
            // 
            // Exit
            // 
            this.Exit.Location = new System.Drawing.Point(992, 27);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(56, 39);
            this.Exit.TabIndex = 1;
            this.Exit.Text = "Exit";
            this.Exit.UseVisualStyleBackColor = true;
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "CHM";
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // PluginStatus
            // 
            this.PluginStatus.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PluginStatus.FormattingEnabled = true;
            this.PluginStatus.ItemHeight = 14;
            this.PluginStatus.Location = new System.Drawing.Point(15, 87);
            this.PluginStatus.Name = "PluginStatus";
            this.PluginStatus.Size = new System.Drawing.Size(387, 88);
            this.PluginStatus.Sorted = true;
            this.PluginStatus.TabIndex = 2;
            // 
            // TimeSliceStatus
            // 
            this.TimeSliceStatus.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TimeSliceStatus.FormattingEnabled = true;
            this.TimeSliceStatus.ItemHeight = 14;
            this.TimeSliceStatus.Location = new System.Drawing.Point(405, 88);
            this.TimeSliceStatus.Name = "TimeSliceStatus";
            this.TimeSliceStatus.Size = new System.Drawing.Size(252, 88);
            this.TimeSliceStatus.TabIndex = 3;
            // 
            // ElapsedTime
            // 
            this.ElapsedTime.BackColor = System.Drawing.SystemColors.Control;
            this.ElapsedTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ElapsedTime.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ElapsedTime.FormattingEnabled = true;
            this.ElapsedTime.ItemHeight = 14;
            this.ElapsedTime.Location = new System.Drawing.Point(29, 49);
            this.ElapsedTime.Name = "ElapsedTime";
            this.ElapsedTime.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.ElapsedTime.Size = new System.Drawing.Size(481, 14);
            this.ElapsedTime.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(133, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Plugin Processes Information";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(457, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Server Processes Information";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(412, 185);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Messages";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(412, 333);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Flags";
            // 
            // FlagBox
            // 
            this.FlagBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FlagBox.FormattingEnabled = true;
            this.FlagBox.ItemHeight = 14;
            this.FlagBox.Location = new System.Drawing.Point(12, 349);
            this.FlagBox.Name = "FlagBox";
            this.FlagBox.Size = new System.Drawing.Size(1036, 116);
            this.FlagBox.Sorted = true;
            this.FlagBox.TabIndex = 8;
            this.FlagBox.DoubleClick += new System.EventHandler(this.FlagBox_DoubleClick);
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugToolStripMenuItem1,
            this.ToolsToolStripMenuItem});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(1060, 24);
            this.MainMenu.TabIndex = 13;
            this.MainMenu.Text = "menuStrip1";
            // 
            // debugToolStripMenuItem1
            // 
            this.debugToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugOnToolStripMenuItem1,
            this.debugOffToolStripMenuItem1,
            this.pluginMonitorToolStripMenuItem,
            this.setDebugVariablesToolStripMenuItem});
            this.debugToolStripMenuItem1.Name = "debugToolStripMenuItem1";
            this.debugToolStripMenuItem1.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem1.Text = "Debug";
            // 
            // debugOnToolStripMenuItem1
            // 
            this.debugOnToolStripMenuItem1.Name = "debugOnToolStripMenuItem1";
            this.debugOnToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.debugOnToolStripMenuItem1.Text = "Debug On";
            this.debugOnToolStripMenuItem1.Click += new System.EventHandler(this.debugOnToolStripMenuItem_Click);
            // 
            // debugOffToolStripMenuItem1
            // 
            this.debugOffToolStripMenuItem1.Name = "debugOffToolStripMenuItem1";
            this.debugOffToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.debugOffToolStripMenuItem1.Text = "Debug Off";
            this.debugOffToolStripMenuItem1.Click += new System.EventHandler(this.debugOffToolStripMenuItem_Click);
            // 
            // pluginMonitorToolStripMenuItem
            // 
            this.pluginMonitorToolStripMenuItem.Name = "pluginMonitorToolStripMenuItem";
            this.pluginMonitorToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.pluginMonitorToolStripMenuItem.Text = "Plugin Monitor";
            this.pluginMonitorToolStripMenuItem.Click += new System.EventHandler(this.pluginMonitorToolStripMenuItem_Click);
            // 
            // setDebugVariablesToolStripMenuItem
            // 
            this.setDebugVariablesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DebugList});
            this.setDebugVariablesToolStripMenuItem.Name = "setDebugVariablesToolStripMenuItem";
            this.setDebugVariablesToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.setDebugVariablesToolStripMenuItem.Text = "Set Debug Variables";
            this.setDebugVariablesToolStripMenuItem.DropDownOpened += new System.EventHandler(this.setDebugVariablesToolStripMenuItem_DropDownOpened);
            // 
            // DebugList
            // 
            this.DebugList.Name = "DebugList";
            this.DebugList.Size = new System.Drawing.Size(121, 23);
            this.DebugList.Click += new System.EventHandler(this.DebugList_Click);
            // 
            // ToolsToolStripMenuItem
            // 
            this.ToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeDatabasePasswordToolStripMenuItem,
            this.exitAndDecryptDatabaseToolStripMenuItem});
            this.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem";
            this.ToolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.ToolsToolStripMenuItem.Text = "Tools";
            // 
            // changeDatabasePasswordToolStripMenuItem
            // 
            this.changeDatabasePasswordToolStripMenuItem.Name = "changeDatabasePasswordToolStripMenuItem";
            this.changeDatabasePasswordToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.changeDatabasePasswordToolStripMenuItem.Text = "Change Database Password";
            this.changeDatabasePasswordToolStripMenuItem.Click += new System.EventHandler(this.changeDatabasePasswordToolStripMenuItem_Click);
            // 
            // exitAndDecryptDatabaseToolStripMenuItem
            // 
            this.exitAndDecryptDatabaseToolStripMenuItem.Name = "exitAndDecryptDatabaseToolStripMenuItem";
            this.exitAndDecryptDatabaseToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.exitAndDecryptDatabaseToolStripMenuItem.Text = "Exit and Decrypt Database";
            this.exitAndDecryptDatabaseToolStripMenuItem.Click += new System.EventHandler(this.exitAndDecryptDatabaseToolStripMenuItem_Click);
            // 
            // ImmediateCommands
            // 
            this.ImmediateCommands.AcceptsReturn = true;
            this.ImmediateCommands.Location = new System.Drawing.Point(3, 22);
            this.ImmediateCommands.Multiline = true;
            this.ImmediateCommands.Name = "ImmediateCommands";
            this.ImmediateCommands.Size = new System.Drawing.Size(530, 51);
            this.ImmediateCommands.TabIndex = 16;
            this.ImmediateCommands.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ImmediateCommands_KeyPress);
            // 
            // Execute
            // 
            this.Execute.Location = new System.Drawing.Point(539, 30);
            this.Execute.Name = "Execute";
            this.Execute.Size = new System.Drawing.Size(75, 23);
            this.Execute.TabIndex = 17;
            this.Execute.Text = "Execute";
            this.Execute.UseVisualStyleBackColor = true;
            this.Execute.Click += new System.EventHandler(this.Execute_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(539, 59);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 18;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // ImmediateCommandsResponse
            // 
            this.ImmediateCommandsResponse.BackColor = System.Drawing.Color.OrangeRed;
            this.ImmediateCommandsResponse.FormattingEnabled = true;
            this.ImmediateCommandsResponse.Location = new System.Drawing.Point(3, 82);
            this.ImmediateCommandsResponse.Name = "ImmediateCommandsResponse";
            this.ImmediateCommandsResponse.Size = new System.Drawing.Size(530, 69);
            this.ImmediateCommandsResponse.TabIndex = 20;
            // 
            // ImmediateCommandResponsePanel
            // 
            this.ImmediateCommandResponsePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.ImmediateCommandResponsePanel.Controls.Add(this.FullDebug);
            this.ImmediateCommandResponsePanel.Controls.Add(this.TestOnly);
            this.ImmediateCommandResponsePanel.Controls.Add(this.label5);
            this.ImmediateCommandResponsePanel.Controls.Add(this.Execute);
            this.ImmediateCommandResponsePanel.Controls.Add(this.Cancel);
            this.ImmediateCommandResponsePanel.Controls.Add(this.ImmediateCommandsResponse);
            this.ImmediateCommandResponsePanel.Controls.Add(this.ImmediateCommands);
            this.ImmediateCommandResponsePanel.Location = new System.Drawing.Point(12, 480);
            this.ImmediateCommandResponsePanel.Name = "ImmediateCommandResponsePanel";
            this.ImmediateCommandResponsePanel.Size = new System.Drawing.Size(628, 160);
            this.ImmediateCommandResponsePanel.TabIndex = 22;
            // 
            // FullDebug
            // 
            this.FullDebug.AutoSize = true;
            this.FullDebug.Location = new System.Drawing.Point(539, 123);
            this.FullDebug.Name = "FullDebug";
            this.FullDebug.Size = new System.Drawing.Size(77, 17);
            this.FullDebug.TabIndex = 23;
            this.FullDebug.Text = "Full Debug";
            this.FullDebug.UseVisualStyleBackColor = true;
            // 
            // TestOnly
            // 
            this.TestOnly.AutoSize = true;
            this.TestOnly.Checked = true;
            this.TestOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.TestOnly.Location = new System.Drawing.Point(539, 100);
            this.TestOnly.Name = "TestOnly";
            this.TestOnly.Size = new System.Drawing.Size(71, 17);
            this.TestOnly.TabIndex = 22;
            this.TestOnly.Text = "Test Only";
            this.TestOnly.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(260, 1);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Immediate Commands";
            // 
            // VoiceBox
            // 
            this.VoiceBox.BackColor = System.Drawing.Color.Red;
            this.VoiceBox.FormattingEnabled = true;
            this.VoiceBox.Location = new System.Drawing.Point(662, 483);
            this.VoiceBox.Name = "VoiceBox";
            this.VoiceBox.Size = new System.Drawing.Size(386, 160);
            this.VoiceBox.TabIndex = 22;
            // 
            // ActionItemsListBox
            // 
            this.ActionItemsListBox.BackColor = System.Drawing.Color.Yellow;
            this.ActionItemsListBox.FormattingEnabled = true;
            this.ActionItemsListBox.Location = new System.Drawing.Point(668, 92);
            this.ActionItemsListBox.Name = "ActionItemsListBox";
            this.ActionItemsListBox.Size = new System.Drawing.Size(380, 82);
            this.ActionItemsListBox.TabIndex = 23;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(820, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Action Items";
            // 
            // UserInterfaceButton
            // 
            this.UserInterfaceButton.Location = new System.Drawing.Point(908, 27);
            this.UserInterfaceButton.Name = "UserInterfaceButton";
            this.UserInterfaceButton.Size = new System.Drawing.Size(61, 39);
            this.UserInterfaceButton.TabIndex = 25;
            this.UserInterfaceButton.Text = "User Interface";
            this.UserInterfaceButton.UseVisualStyleBackColor = true;
            this.UserInterfaceButton.Click += new System.EventHandler(this.UserInterfaceButton_Click);
            // 
            // MainCHMForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(1060, 653);
            this.Controls.Add(this.UserInterfaceButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ActionItemsListBox);
            this.Controls.Add(this.VoiceBox);
            this.Controls.Add(this.ImmediateCommandResponsePanel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.FlagBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ElapsedTime);
            this.Controls.Add(this.TimeSliceStatus);
            this.Controls.Add(this.PluginStatus);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.CHMMainFormMessageBox);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MainMenu;
            this.Name = "MainCHMForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CHMMainForm";
            this.Activated += new System.EventHandler(this.MainCHMForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainCHMForm_FormClosing);
            this.Load += new System.EventHandler(this.MainCHMForm_Load);
            this.Shown += new System.EventHandler(this.MainCHMForm_Shown);
            this.Resize += new System.EventHandler(this.MainCHMForm_Resize);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.ImmediateCommandResponsePanel.ResumeLayout(false);
            this.ImmediateCommandResponsePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox CHMMainFormMessageBox;
        private System.Windows.Forms.Button Exit;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ListBox PluginStatus;
        private System.Windows.Forms.ListBox TimeSliceStatus;
        private System.Windows.Forms.ListBox ElapsedTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox FlagBox;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem debugOffToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pluginMonitorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugOnToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem setDebugVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox DebugList;
        private System.Windows.Forms.ToolStripMenuItem ToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeDatabasePasswordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitAndDecryptDatabaseToolStripMenuItem;
        private System.Windows.Forms.TextBox ImmediateCommands;
        private System.Windows.Forms.Button Execute;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.ListBox ImmediateCommandsResponse;
        private System.Windows.Forms.Panel ImmediateCommandResponsePanel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox VoiceBox;
        private System.Windows.Forms.CheckBox TestOnly;
        private System.Windows.Forms.CheckBox FullDebug;
        private System.Windows.Forms.ListBox ActionItemsListBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button UserInterfaceButton;
    }
}

