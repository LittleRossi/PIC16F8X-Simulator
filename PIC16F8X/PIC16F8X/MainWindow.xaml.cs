using Microsoft.Win32;
using PIC16F8X.DataModel;
using PIC16F8X.ViewModel;
using System.Windows;

namespace PIC16F8X
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel View;

        public MainWindow()
        {

            // DataContext for UI
            View = new MainWindowViewModel();
            DataContext = View;

            //TODO


            // END TODO


            // initialize UI
            InitializeComponent();

            // Set initial Data
            Reset();
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
                Reset(); // reset all Data and update UI
                LSTFile lSTFile = new LSTFile(dialog.FileName); // read and initialise programm in programmstorage as list of Commands
                
                //ToDo: show programm in UI
            }
        }


        #region User interaction functions
        private void Reset()
        {
            Fields.ResetData();
            // ToDo:
            //  - stop
            //  - UI Update

        }
        #endregion


    }
}
