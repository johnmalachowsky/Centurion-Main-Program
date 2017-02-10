using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CHM
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = "Local\\" +System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            using (Mutex mutex = new Mutex(false, mutexName))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Instance of application already running!", "Centurion Home Monitor");
                    return;
                }

                GC.Collect();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainCHMForm());
                GC.KeepAlive(mutex);
            }
        }
    }
}
