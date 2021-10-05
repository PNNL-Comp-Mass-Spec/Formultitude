using System;
using System.Windows.Forms;

namespace CiaUi
{
    internal static class FormularityUIProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormularityUIForm());
            }
            catch (Exception ex)
            {
                var sss = ex.Message;
            }
        }
    }
}
