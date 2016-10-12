using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHM
{
    public partial class TimedDialogBoxWithListbox: Form
    {
        string T_Title, T_Message;
        int T_SecondsToDisplay;
        List<string> T_ListBoxToDisplay;

        public TimedDialogBoxWithListbox(string Title, string Message, List<string> ListToDisplay, int SecondsToDisplay)
        {
            InitializeComponent();
            T_Title = Title;
            T_Message = Message;
            T_SecondsToDisplay = SecondsToDisplay;
            T_ListBoxToDisplay = ListToDisplay;

        }

        private void TDBWL_Button_Click(object sender, EventArgs e)
        {
            Close();

        }

        private void TimedDialogBoxWithListbox_Shown(object sender, EventArgs e)
        {
            this.Text = T_Title;
            for (int i = 150; i < T_Message.Length; i = i + 150)
            {
                T_Message = T_Message.Substring(0, i) + "\r\n" + T_Message.Substring(i);
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
        }
       
    }
}
