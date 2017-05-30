using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CiaUi {
    static class CIAUIProgram {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault( false );
                Application.Run( new CIAUIForm() );
            } catch( Exception ex ) {
                string sss = ex.Message;
            }
        }
    }
}
