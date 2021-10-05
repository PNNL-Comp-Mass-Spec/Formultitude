using System;
using System.Windows.Forms;

namespace CiaUi {
    static class FormularityUIProgram {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                Application.Run( new FormularityUIForm() );
            } catch( Exception ex ) {
                string sss = ex.Message;
            }
        }
    }
}
