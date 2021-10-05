using System;
using System.Windows.Forms;

namespace FindChains
{
    static class FindChainsProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FindChainsForm());
        }
    }
}
