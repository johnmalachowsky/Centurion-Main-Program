using System;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHM
{
    public partial class ChangePasswordForm : Form
    {
        private string PreviousPassword = "";
        public string CurrentPassword="";
        public string BrandNewPassword="";
        public bool ResultFlag=false;

        public ChangePasswordForm(string PasswordType, bool AskForCurrentPassword)
        {
            InitializeComponent();
            this.Text = "Change " + PasswordType + " Password";
            label1.Text = "Old " + PasswordType + " Password";
            label2.Text = "New " + PasswordType + " Password";
            label3.Text = "Retype New " + PasswordType + " Password";
            if(!AskForCurrentPassword)
            {
                label1.Visible = false;
                OldPassword.Visible = false;
            }
        }


         private int CheckPasswordWithDetails(string pwd, out string Complexity, out int RequirementsCount)
        {
            try
            {
                // Init Vars
                int nScore = 0;
                string sComplexity = "";
                int iUpperCase = 0;
                int iLowerCase = 0;
                int iDigit = 0;
                int iSymbol = 0;
                int iRepeated = 1;
                Hashtable htRepeated = new Hashtable();
                int iMiddle = 0;
                int iMiddleEx = 1;
                int ConsecutiveMode = 0;
                int iConsecutiveUpper = 0;
                int iConsecutiveLower = 0;
                int iConsecutiveDigit = 0;
                int iLevel = 0;
                string sAlphas = "abcdefghijklmnopqrstuvwxyz";
                string sNumerics = "01234567890";
                int nSeqAlpha = 0;
                int nSeqChar = 0;
                int nSeqNumber = 0;

                // Scan password
                foreach (char ch in pwd.ToCharArray())
                {
                    // Count digits
                    if (Char.IsDigit(ch))
                    {
                        iDigit++;

                        if (ConsecutiveMode == 3)
                            iConsecutiveDigit++;
                        ConsecutiveMode = 3;
                    }

                    // Count uppercase characters
                    if (Char.IsUpper(ch))
                    {
                        iUpperCase++;
                        if (ConsecutiveMode == 1)
                            iConsecutiveUpper++;
                        ConsecutiveMode = 1;
                    }

                    // Count lowercase characters
                    if (Char.IsLower(ch))
                    {
                        iLowerCase++;
                        if (ConsecutiveMode == 2)
                            iConsecutiveLower++;
                        ConsecutiveMode = 2;
                    }

                    // Count symbols
                    if (Char.IsSymbol(ch) || Char.IsPunctuation(ch))
                    {
                        iSymbol++;
                        ConsecutiveMode = 0;
                    }

                    // Count repeated letters 
                    if (Char.IsLetter(ch))
                    {
                        if (htRepeated.Contains(Char.ToLower(ch))) iRepeated++;
                        else htRepeated.Add(Char.ToLower(ch), 0);

                        if (iMiddleEx > 1)
                            iMiddle = iMiddleEx - 1;
                    }

                    if (iUpperCase > 0 || iLowerCase > 0)
                    {
                        if (Char.IsDigit(ch) || Char.IsSymbol(ch))
                            iMiddleEx++;
                    }
                }

                // Check for sequential alpha string patterns (forward and reverse) 
                for (int s = 0; s < 23; s++)
                {
                    string sFwd = sAlphas.Substring(s, 3);
                    string sRev = strReverse(sFwd);
                    if (pwd.ToLower().IndexOf(sFwd) != -1 || pwd.ToLower().IndexOf(sRev) != -1)
                    {
                        nSeqAlpha++;
                        nSeqChar++;
                    }
                }

                // Check for sequential numeric string patterns (forward and reverse)
                for (int s = 0; s < 8; s++)
                {
                    string sFwd = sNumerics.Substring(s, 3);
                    string sRev = strReverse(sFwd);
                    if (pwd.ToLower().IndexOf(sFwd) != -1 || pwd.ToLower().IndexOf(sRev) != -1)
                    {
                        nSeqNumber++;
                        nSeqChar++;
                    }
                }

                // Calcuate score
                // Score += 4 * Password Length
                nScore = 4 * pwd.Length;

                // if we have uppercase letetrs Score +=(number of uppercase letters *2)
                if (iUpperCase > 0)
                {
                    nScore += ((pwd.Length - iUpperCase) * 2);
                }

                // if we have lowercase letetrs Score +=(number of lowercase letters *2)
                if (iLowerCase > 0)
                {
                    nScore += ((pwd.Length - iLowerCase) * 2);
                }

                // Score += (Number of digits *4)
                nScore += (iDigit * 4);

                // Score += (Number of Symbols * 6)
                nScore += (iSymbol * 6);

                // Score += (Number of digits or symbols in middle of password *2)
                nScore += (iMiddle * 2);

                //requirments
                int requirments = 0;
                if (pwd.Length >= 8) requirments++;     // Min password length
                if (iUpperCase > 0) requirments++;      // Uppercase letters
                if (iLowerCase > 0) requirments++;      // Lowercase letters
                if (iDigit > 0) requirments++;          // Digits
                if (iSymbol > 0) requirments++;         // Symbols

                // If we have more than 3 requirments then
                if (requirments > 3)
                {
                    // Score += (requirments *2) 
                    nScore += (requirments * 2);
                }

                //
                // Deductions
                //

                // If only letters then score -=  password length
                if (iDigit == 0 && iSymbol == 0)
                {
                    nScore -= pwd.Length;
                }

                // If only digits then score -=  password length
                if (iDigit == pwd.Length)
                {
                    nScore -= pwd.Length;
                }

                // If repeated letters used then score -= (iRepeated * (iRepeated - 1));
                if (iRepeated > 1)
                {
                    nScore -= (iRepeated * (iRepeated - 1));
                }

                // If Consecutive uppercase letters then score -= (iConsecutiveUpper * 2);
                nScore -= (iConsecutiveUpper * 2);

                // If Consecutive lowercase letters then score -= (iConsecutiveUpper * 2);
                nScore -= (iConsecutiveLower * 2);

                // If Consecutive digits used then score -= (iConsecutiveDigits* 2);
                nScore -= (iConsecutiveDigit * 2);

                // If password contains sequence of letters then score -= (nSeqAlpha * 3)
                nScore -= (nSeqAlpha * 3);

                // If password contains sequence of digits then score -= (nSeqNumber * 3)
                nScore -= (nSeqNumber * 3);

                /* Determine complexity based on overall score */
                if (nScore > 100) { nScore = 100; } else if (nScore < 0) { nScore = 0; }
                if (nScore >= 0 && nScore < 20) { sComplexity = "Very Weak"; }
                else if (nScore >= 20 && nScore < 40) { sComplexity = "Weak"; }
                else if (nScore >= 40 && nScore < 60) { sComplexity = "Good"; }
                else if (nScore >= 60 && nScore < 80) { sComplexity = "Strong"; }
                else if (nScore >= 80 && nScore <= 100) { sComplexity = "Very Strong"; }

                Complexity = sComplexity;
                RequirementsCount = requirments;
                return (nScore);
            }
            catch
            {
                Complexity = "";
                RequirementsCount = 0;
                return (0);
             }
  

        }

        private string strReverse(string str)
        {
            try
            {
                string newstring = "";
                for (int s = 0; s < str.Length; s++)
                {
                    newstring = str[s] + newstring;
                }
                return newstring;
            }
            catch 
            {

                return ("");
            }
        }

        private void NewPassword_TextChanged(object sender, EventArgs e)
        {
            if (PreviousPassword != NewPassword.Text)
            {
                string Strength;
                int Require;
                PreviousPassword = NewPassword.Text;
                int level = CheckPasswordWithDetails(PreviousPassword, out Strength, out Require);
                PasswordStrength.Text = Strength;
                if (Require < 5)
                {
                    Requirements.BackColor = Color.Red;
                    Requirements.Text = "Types " + Require.ToString() + "/5";
                }
                else
                {
                    Requirements.Text = "Ready";
                    Requirements.BackColor = Color.Green;
                }
                if (level < 26)
                    PasswordStrength.BackColor = Color.Red;
                if (level >=80)
                    PasswordStrength.BackColor = Color.Green;
                if(level>=26 && level<80)
                    PasswordStrength.BackColor = Color.Yellow;

                ReenterPassword_TextChanged(sender, e);
            }
        }

        private void ChangePasswordForm_Shown(object sender, EventArgs e)
        {
            PreviousPassword="OLD";
            NewPassword.Text = "";
            NewPassword_TextChanged(sender, e);
            ReenterPassword_TextChanged(sender, e);
        }


        private void ReenterPassword_TextChanged(object sender, EventArgs e)
        {
            if (NewPassword.Text == ReenterPassword.Text && NewPassword.Text.Length > 0)
            {
                DuplicateOK.Text = "Matches";
                DuplicateOK.BackColor = Color.Green;
            }
            else
            {
                DuplicateOK.Text = "Does Not Match";
                DuplicateOK.BackColor = Color.Red;
            }

            if (DuplicateOK.BackColor == Color.Green && PasswordStrength.BackColor == Color.Green)
                Save.Visible = true;
            else
                Save.Visible = false;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            CurrentPassword=OldPassword.Text;
            BrandNewPassword=NewPassword.Text;
            ResultFlag = true;
            Close();

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            CurrentPassword = "";
            BrandNewPassword = "";
            ResultFlag = false;
            Close();

        }

    }
}
