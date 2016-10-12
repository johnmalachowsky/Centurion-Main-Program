using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CHM
{
    public partial class EnterPasswordForm : Form
    {
        public string EnteredPassword;
        public bool Saved=false;
         
        public EnterPasswordForm( string PasswordBoxTitle)
        {
            InitializeComponent();
            this.Text=PasswordBoxTitle;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            EnteredPassword="";
            Saved=false;
            this.Close();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            EnteredPassword = Password.Text;
            Saved = true;
            this.Close(); 
        }

   }
}
