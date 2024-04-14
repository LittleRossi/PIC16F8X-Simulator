using Microsoft.Win32;
using PIC16F8X.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PIC16F8X
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }


        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Öffne LST File",
                Filter = "LST files (*.LST)|*.LST",
                RestoreDirectory = true,
            };

            if (dialog.ShowDialog() == true)
            {
                Reset(); // reset all Data
                LSTFile lSTFile = new LSTFile(dialog.FileName); // read and initialise programm in programmstorage as list of Commands
                
                //ToDo: Programm in ProgrammView in UI anzeigen
            }
        }


        #region User interaction functions
        private void Reset()
        {
            // ToDo
        }
        #endregion


    }
}
