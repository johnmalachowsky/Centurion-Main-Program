using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    class ParseCommandString
    {
        private string LastError;
        private int LastErrorNumber;
        private List<TokenInfoStruct> FlagList;
        private List<TokenInfoStruct> SymbolList;
        private List<TokenInfoStruct> CommandList;
        private List<TokenInfoStruct> DeviceList;
        private List<TokenInfoStruct> TemporalList;
     

        IEnumerable<TokenInfoStruct> newList = null;

        struct TokenInfoStruct
        {
            internal string TokenName;
            internal string Uniqueid;
        }

        public ParseCommandString()
        {
            TokenInfoStruct f1;

            TemporalList = new List<TokenInfoStruct>();

            f1.TokenName = " " + "Every" + " ";
            f1.Uniqueid = "T00001";
            TemporalList.Add(f1);

            f1.TokenName = " " + "From" + " ";
            f1.Uniqueid = "T00002";
            TemporalList.Add(f1);

            f1.TokenName = " " + "Until" + " ";
            f1.Uniqueid = "T00003";
            TemporalList.Add(f1);

            f1.TokenName = " " + "During" + " ";
            f1.Uniqueid = "T00004";
            TemporalList.Add(f1);

            f1.TokenName = " " + "In" + " ";
            f1.Uniqueid = "T00005";
            TemporalList.Add(f1);

            f1.TokenName = " " + "On" + " ";
            f1.Uniqueid = "T00006";  //Before Then!!!!!!!!!!!!
            TemporalList.Add(f1);


            



            FlagList = new List<TokenInfoStruct>();

            f1.TokenName = " " + "living room light" + " ";
            f1.Uniqueid = "00001";
            FlagList.Add(f1);

            f1.TokenName = " " + "living room fan" + " ";
            f1.Uniqueid = "00002";
            FlagList.Add(f1);

            f1.TokenName = " " + "living room temperature" + " ";
            f1.Uniqueid = "00003";
            FlagList.Add(f1);

            f1.TokenName = " " + "outside temperature" + " ";
            f1.Uniqueid = "00004";
            FlagList.Add(f1);

            f1.TokenName = " " + "outside lights" + " ";
            f1.Uniqueid = "00005";
            FlagList.Add(f1);

            f1.TokenName = " " + "outside light" + " ";
            f1.Uniqueid = "00006";
            FlagList.Add(f1);

            f1.TokenName = " " + "front door lock" + " ";
            f1.Uniqueid = "00007";
            FlagList.Add(f1);

            f1.TokenName = " " + "back door lock" + " ";
            f1.Uniqueid = "00008";
            FlagList.Add(f1);


            SymbolList = new List<TokenInfoStruct>();

            f1.TokenName = " " + "=" + " ";
            f1.Uniqueid = "S00001";
            SymbolList.Add(f1);

            f1.TokenName = " " + "==" + " ";
            f1.Uniqueid = "S00002";
            SymbolList.Add(f1);

            f1.TokenName = " " + "<" + " ";
            f1.Uniqueid = "S00003";
            SymbolList.Add(f1);

            f1.TokenName = " " + ">" + " ";
            f1.Uniqueid = "S00004";
            SymbolList.Add(f1);

            f1.TokenName = " " + "<=" + " ";
            f1.Uniqueid = "S00005";
            SymbolList.Add(f1);

            f1.TokenName = " " + "=<" + " ";
            f1.Uniqueid = "S00005";
            SymbolList.Add(f1);

            f1.TokenName = " " + ">=" + " ";
            f1.Uniqueid = "S00006";
            SymbolList.Add(f1);

            f1.TokenName = " " + "=>" + " ";
            f1.Uniqueid = "S00006";
            SymbolList.Add(f1);

            CommandList = new List<TokenInfoStruct>();
            f1.TokenName = " " + "on" + " ";
            f1.Uniqueid = "C00001";
            CommandList.Add(f1);

            f1.TokenName = " " + "off" + " ";
            f1.Uniqueid = "C00002";
            CommandList.Add(f1);

            f1.TokenName = " " + "open" + " ";
            f1.Uniqueid = "C00003";
            CommandList.Add(f1);

            f1.TokenName = " " + "close" + " ";
            f1.Uniqueid = "C00004";
            CommandList.Add(f1);

            f1.TokenName = " " + "lock" + " ";
            f1.Uniqueid = "C00005";
            CommandList.Add(f1);

            f1.TokenName = " " + "unlock" + " ";
            f1.Uniqueid = "C00006";
            CommandList.Add(f1);

            f1.TokenName = " " + "dim" + " ";
            f1.Uniqueid = "C00007";
            CommandList.Add(f1);

            f1.TokenName = " " + "decrease" + " ";
            f1.Uniqueid = "C00007";
            CommandList.Add(f1);

            f1.TokenName = " " + "lower" + " ";
            f1.Uniqueid = "C00007";
            CommandList.Add(f1);

            f1.TokenName = " " + "brighten" + " ";
            f1.Uniqueid = "C00008";
            CommandList.Add(f1);

            f1.TokenName = " " + "increase" + " ";
            f1.Uniqueid = "C00008";
            CommandList.Add(f1);

            f1.TokenName = " " + "raise" + " ";
            f1.Uniqueid = "C00008";
            CommandList.Add(f1);

            DeviceList = new List<TokenInfoStruct>();
            f1.TokenName = " " + "all lights" + " ";
            f1.Uniqueid = "D00001";
            DeviceList.Add(f1);

            f1.TokenName = " " + "all devices" + " ";
            f1.Uniqueid = "D00002";
            DeviceList.Add(f1);

            f1.TokenName = " " + "all door locks" + " ";
            f1.Uniqueid = "D00003";
            DeviceList.Add(f1);

            f1.TokenName = " " + "all fans" + " ";
            f1.Uniqueid = "D00004";
            DeviceList.Add(f1);


        }
           
 


        internal string GetLastErrorWords()
        {
            return (LastError);
        }
        internal int GetLastErrorNumber()
        {
            return (LastErrorNumber);
        }


        /// <summary>
        /// Produce formatted string by the given string
        /// 1-provides spaces around symbols
        /// 2-removes extra spaces
        /// 3-turns it to all lower case
        /// </summary>
        /// <param name="expression">Unformatted  expression</param>
        /// <param name="FormattedString">Formatted expression</param>
        /// <returns>true if completes successfully, false if error (retrieved by calling GetLastErrorWords/Number</returns>
        internal bool FormatString(string expression, out string FormattedString)
        {
            FormattedString = "";
            LastError = "";
            LastErrorNumber = 0;

            if (string.IsNullOrEmpty(expression))
            {
                LastError = "Expression is null or empty";
                LastErrorNumber = 1;
                return (false);
            }

            StringBuilder formattedString = new StringBuilder();
            int balanceOfParenth = 0; // Check number of parenthesis
            int balanceOfBrackets = 0; //Check Brackets
            int balanceOfBraces = 0; //Check Braces

            expression = Regex.Replace(expression, @"\s+", " "); //Remove Duplicate Spaces

            // Format string in one iteration and check number of parenthesis
            // (this function do 2 tasks because performance priority)
            
            char ch, ch1, ch0, ch2;
            for (int i = 0; i < expression.Length; i++)
            {
                ch = expression[i];
                if (i + 1 < expression.Length)
                    ch1 = expression[i + 1];
                else
                    ch1 = '\0';

                if (i + 2 < expression.Length)
                    ch2 = expression[i + 2];
                else
                    ch2 = '\0';

                if (i > 0)
                    ch0 = expression[i - 1];
                else
                    ch0 = '\0';

                if (ch == '(')
                {
                    balanceOfParenth++;
                    formattedString.Append(ch);
                    continue;
                }
                if (ch == ')')
                {
                    balanceOfParenth--;
                    formattedString.Append(ch);
                    continue;
                }

                if (ch == '{')
                {
                    balanceOfBraces++;
                    formattedString.Append(ch);
                    continue;
                }
                if (ch == '}')
                {
                    balanceOfBraces--;
                    formattedString.Append(ch);
                    continue;
                }

                if (ch == '[')
                {
                    balanceOfBrackets++;
                    formattedString.Append(ch);
                    continue;
                }
                if (ch == ']')
                {
                    balanceOfBrackets--;
                    formattedString.Append(ch);
                    continue;
                }

                if(Char.IsSymbol(ch))
                {
                    if(!Char.IsSymbol(ch0))
                        formattedString.Append(" ");
                    formattedString.Append(ch);
                    if (Char.IsSymbol(ch2))
                    {
                        formattedString.Append(ch2);
                        formattedString.Append(" ");
                        i = i + 2;
                        continue;
                    }
                    if (!Char.IsSymbol(ch1))
                        formattedString.Append(" ");
                    continue;
                }

                if (Char.IsUpper(ch))
                {
                    formattedString.Append(Char.ToLower(ch));
                }
                else
                {
                    formattedString.Append(ch);
                }
            }

            if (balanceOfParenth != 0)
            {
                LastError = "Number of left and right parenthesis '()' is not equal";
                LastErrorNumber = 2;
                return (false);
            }

            if (balanceOfBraces != 0)
            {
                LastError = "Number of left and right braces '{}' is not equal";
                LastErrorNumber = 3;
                return (false);
            }
            
            if (balanceOfParenth != 0)
            {
                LastError = "Number of left and right balanceOfBrackets '[]' is not equal";
                LastErrorNumber = 4;
                return (false);
            }

            FormattedString = Regex.Replace(formattedString.ToString(), @"\s+", " "); //Remove Duplicate Spaces
            char last = FormattedString[FormattedString.Length - 1];
            if (Char.IsPunctuation(last))
            {
                FormattedString = FormattedString.Remove(FormattedString.Length - 1, 1) + " ";
            }
            //if(FormattedString.StartsWith("on "))//Prevents confusion with on command
            //    FormattedString="if "+FormattedString.Substring(3);
            FormattedString = FormattedString + " ";
            FormattedString = Regex.Replace(FormattedString.ToString(), @"\s+", " "); //Remove Duplicate Spaces
            

            return (true);
        }


        /// <summary>
        /// Find Any Symbols and Enclose between \x01 and \x02
        /// </summary>
        /// <param name="expression">Unformatted Command String</param>
        /// <param name="FormattedString">Processed Command String</param>
        /// <returns>true if completes successfully, false if error (retrieved by calling GetLastErrorWords/Number</returns>
        internal bool FindSymbolReferences(string expression, out string FormattedString)
        {
            FormattedString = "";
            LastError = "";
            LastErrorNumber = 0;

            if (string.IsNullOrEmpty(expression))
            {
                LastError = "Expression is null or empty";
                LastErrorNumber = 1;
                return (false);
            }

            expression = Regex.Replace(expression, @"\s+", " "); //Remove Duplicate Spaces

            newList = SymbolList.OrderByDescending(x => x.TokenName.Length);
            StringBuilder formattedString = new StringBuilder(expression);

            foreach (TokenInfoStruct SL in newList)
            {
                int command=Math.Max(formattedString.ToString().IndexOf("then"),0);
                formattedString.Replace(SL.TokenName, " \x01" + SL.Uniqueid + "\x02 ", command, formattedString.ToString().Length-command);
            }
            FormattedString = Regex.Replace(formattedString.ToString(), @"\s+", " "); //Remove Duplicate Spaces
            return (true);


        }   

        /// <summary>
        /// Find Any Flag References and Enclose between \x03 and \x04
        /// </summary>
        /// <param name="expression">Unformatted Command String</param>
        /// <param name="FormattedString">Processed Command String</param>
        /// <returns>true if completes successfully, false if error (retrieved by calling GetLastErrorWords/Number</returns>
        internal bool FindFlagReferences(string expression, out string FormattedString)
        {
            FormattedString = "";
            LastError = "";
            LastErrorNumber = 0;

            if (string.IsNullOrEmpty(expression))
            {
                LastError = "Expression is null or empty";
                LastErrorNumber = 1;
                return (false);
            }
            expression = Regex.Replace(expression, @"\s+", " "); //Remove Duplicate Spaces

            newList = FlagList.OrderByDescending(x => x.TokenName.Length);
            StringBuilder formattedString = new StringBuilder(expression);

            foreach (TokenInfoStruct FL in newList)
            {
                formattedString.Replace(FL.TokenName, " \x03"+FL.Uniqueid+"\x04 ");
            }
            FormattedString = Regex.Replace(formattedString.ToString(), @"\s+", " "); //Remove Duplicate Spaces
            return (true);

  
        }
        /// <summary>
        /// Find Any Command References and Enclose between \x05 and \x06        /// </summary>
        /// <param name="expression">Unformatted Command String</param>
        /// <param name="FormattedString">Processed Command String</param>
        /// <returns>true if completes successfully, false if error (retrieved by calling GetLastErrorWords/Number</returns>
        internal bool FindCommandReferences(string expression, out string FormattedString)
        {
            FormattedString = "";
            LastError = "";
            LastErrorNumber = 0;

            if (string.IsNullOrEmpty(expression))
            {
                LastError = "Expression is null or empty";
                LastErrorNumber = 1;
                return (false);
            }
            expression = Regex.Replace(expression, @"\s+", " "); //Remove Duplicate Spaces

            newList = CommandList.OrderByDescending(x => x.TokenName.Length);
            StringBuilder formattedString = new StringBuilder(expression);

            foreach (TokenInfoStruct FL in newList)
            {
                formattedString.Replace(FL.TokenName, " \x05" + FL.Uniqueid + "\x06 ");
            }
            FormattedString = Regex.Replace(formattedString.ToString(), @"\s+", " "); //Remove Duplicate Spaces
            return (true);
        }

        /// <summary>
        /// Find Any Device References and Enclose between \x07 and \x08        /// </summary>
        /// <param name="expression">Unformatted Command String</param>
        /// <param name="FormattedString">Processed Command String</param>
        /// <returns>true if completes successfully, false if error (retrieved by calling GetLastErrorWords/Number</returns>
        internal bool FindDeviceReferences(string expression, out string FormattedString)
        {
            FormattedString = "";
            LastError = "";
            LastErrorNumber = 0;

            if (string.IsNullOrEmpty(expression))
            {
                LastError = "Expression is null or empty";
                LastErrorNumber = 1;
                return (false);
            }
            expression = Regex.Replace(expression, @"\s+", " "); //Remove Duplicate Spaces

            newList = DeviceList.OrderByDescending(x => x.TokenName.Length);
            StringBuilder formattedString = new StringBuilder(expression);

            foreach (TokenInfoStruct FL in newList)
            {
                formattedString.Replace(FL.TokenName, " \x07" + FL.Uniqueid + "\x08 ");
            }
            FormattedString = Regex.Replace(formattedString.ToString(), @"\s+", " "); //Remove Duplicate Spaces
            return (true);
        }

        /// <summary>
        /// Parse expression into action/consition blocks
        /// </summary>
        /// <param name="expression">Unformatted Command String</param>
        /// <param name="Conditions">string[] with all the conditions</param>
        /// <param name="Conditions">string[] with all the required actions</param>
        /// <returns>true if completes successfully, false if error (retrieved by calling GetLastErrorWords/Number</returns>
        internal bool ParseIntoProcessingUnits(string expression, out string[] Conditions, out string[] Actions)
        {
            string[] _Conditions=null;
            string[] _Actions = null;
            string _LocalString, _LocalConditions, _LocalActions;
            int a, b, c;
            int Cond, Act;

            if (string.IsNullOrEmpty(expression))
            {
                LastError = "Expression is null or empty";
                LastErrorNumber = 1;
                Conditions = _Conditions;
                Actions = _Actions;
                return (false);
            }

            expression = Regex.Replace(expression, @"\s+", " "); //Remove Duplicate Spaces
            _LocalString=expression;

            a=_LocalString.IndexOf("if");
            b = _LocalString.IndexOf("when");
            c = _LocalString.IndexOf("at");
            Act = _LocalString.IndexOf("then");

            if (a>=0 || b>=0 || c>=0)
            {
                a = Math.Max(a, b);
                Cond = Math.Max(a, c);
                _LocalConditions = _LocalString.Substring(Cond, Act-Cond).Trim();
                _LocalActions = _LocalString.Substring(Act + 4).Trim();
            }
            else //Just a command String
            {
                Act = 0;
                Cond = -1;
                _LocalActions = _LocalString;
            }

            Conditions = _Conditions;
            Actions = _Actions;
            return (false);
        }
    
    }
}
