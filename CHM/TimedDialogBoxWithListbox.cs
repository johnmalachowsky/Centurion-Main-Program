using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;


namespace CHM
{
    public partial class TimedDialogBoxWithListbox: Form
    {
        string T_Title, T_Message;
        int T_SecondsToDisplay;
        List<string> T_ListBoxToDisplay;
        string T_LogFileLocation = "";

        public TimedDialogBoxWithListbox(string Title, string Message, List<string> ListToDisplay, int SecondsToDisplay)
        {
            InitializeComponent();
            T_Title = Title;
            T_Message = Message;
            T_SecondsToDisplay = SecondsToDisplay;
            T_ListBoxToDisplay = ListToDisplay;

        }

        public TimedDialogBoxWithListbox(string Title, string Message, List<string> ListToDisplay, int SecondsToDisplay, string LogFileLocation)
        {
            InitializeComponent();
            T_Title = Title;
            T_Message = Message;
            T_SecondsToDisplay = SecondsToDisplay;
            T_ListBoxToDisplay = ListToDisplay;
            T_LogFileLocation = LogFileLocation;
            TDBWL_Button2.Visible = true;

        }

        private void TDBWL_Button_Click(object sender, EventArgs e)
        {
            Close();

        }

        private void TDBWL_Button2_Click(object sender, EventArgs e)
        {
            try
            {
                string PathToWrite = Path.Combine(T_LogFileLocation, T_Title + DateTime.Now.ToString("yyyyMMddHHmmss")+".txt");

                System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(PathToWrite);
                foreach (var item in TDBWL_listBox.Items)
                {
                    SaveFile.WriteLine(item.ToString());
                }
                SaveFile.Close();
                TDBWL_Button2.Text = "Saved";
                TDBWL_Button2.Enabled = false;
            }
            catch(Exception err)
            {
                TDBWL_Button2.Text = "Error-"+err.HResult;
                TDBWL_Button2.Enabled = false;

            }
        }

        private void TimedDialogBoxWithListbox_Shown(object sender, EventArgs e)
        {
            this.Text = T_Title;
            bool split = true;
            while (split)
            {
                string[] lines = T_Message.Split("\r\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                split = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Length > 150)
                    {
                        lines[i] = lines[i].Substring(0, 150) + "\r\n" + lines[i].Substring(150);
                        split = true;
                    }
                }
                T_Message = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    T_Message = T_Message + lines[i] + "\r\n";
                }
            }
            TimedDialogBoxStuff.Text = T_Message;



            TDBWL_listBox.DataSource = T_ListBoxToDisplay;
            Size sq = new Size(TDBWL_listBox.Width*2, TDBWL_listBox.Height);
            TDBWL_listBox.Size = sq;
            Size s = new Size(this.Width, this.Height + 60);
            this.Size = s;
            int x = Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2;
            int y = Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2;
            this.Location = new Point(x, y);
            Size sq2 = new Size(this.Width-2, TDBWL_listBox.Height);
            TDBWL_listBox.Size = sq2;
        }
       
    }
}
