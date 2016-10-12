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
    public partial class TimedDialogBox : Form
    {
        string T_Title, T_Message;
        int T_SecondsToDisplay;


        public TimedDialogBox(string Title, string Message, int SecondsToDisplay)
        {
            InitializeComponent();
            T_Title = Title;
            T_Message = Message;
            T_SecondsToDisplay = SecondsToDisplay;
        }

        private void TDB_button_Click(object sender, EventArgs e)
        {
            Close();

        }

        private void TimedDialogBox_Shown(object sender, EventArgs e)
        {
            this.Text = T_Title;
            for (int i=150; i < T_Message.Length; i=i+150)
            {
                T_Message = T_Message.Substring(0,i) + "\r\n" + T_Message.Substring(i);
            }
            TimedDialogBoxStuff.Text = T_Message;
 
           Size s = new Size(this.Width, this.Height + 60);
            this.Size = s;
            int x = Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2;
            int y = Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2;
            this.Location = new Point(x, y);
        }
    }
}
