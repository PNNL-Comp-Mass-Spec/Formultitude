using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileReader {
    public partial class FileReaderForm : Form {
        public FileReaderForm() {
            InitializeComponent();
        }

        private void FileReaderForm_DragEnter( object sender, DragEventArgs e ) {
            if( e.Data.GetDataPresent( DataFormats.FileDrop ) == true ) {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void FileReaderForm_DragDrop( object sender, DragEventArgs e ) {
            textBoxResult.Text = string.Empty;
            string [] Filenames = ( string [] ) e.Data.GetData( DataFormats.FileDrop );

            double [] MZs;
            double [] Abundances;
            double [] SNs;
            double [] Resolutions;
            double [] RelAbundances;

            try {
                FileReader.ReadFile( Filenames [ 0 ], out MZs, out Abundances, out SNs, out Resolutions, out RelAbundances );
            } catch( Exception ex ) {
                textBoxResult.Text = ex.Message;
                return;
            }
            textBoxResult.Text = "OK";
        }
    }
}
