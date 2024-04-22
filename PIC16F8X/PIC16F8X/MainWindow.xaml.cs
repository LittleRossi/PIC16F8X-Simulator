using Microsoft.Win32;
using PIC16F8X.DataModel;
using PIC16F8X.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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


        #region Textbox functions
        private void TextBlock_StatusBitChange(object sender, MouseButtonEventArgs e)
        {
            TextBlock tBlock = (TextBlock)sender;
            int bit = (int)typeof(Flags.Status).GetField(tBlock.Name).GetValue(this); // gets the bit-index of the matching Status register
            Fields.ToggleSingleRegisterBit(Registers.STATUS, bit); // toggle the bit value
            
            //ToDo: Update UI
        }

        private void TextBlock_OptionBitChange(object sender, MouseButtonEventArgs e)
        {
            TextBlock tBlock = (TextBlock)sender;
            int bit = (int)typeof(Flags.Option).GetField(tBlock.Name).GetValue(this); // gets the bit-index of the matching Option register
            Fields.ToggleSingleRegisterBit(Registers.OPTION, bit); // toggle the bit value

            //ToDo: Update UI
        }

        private void TextBlock_IntconBitChange(object sender, MouseButtonEventArgs e)
        {
            TextBlock tBlock = (TextBlock)sender;
            int bit = (int)typeof(Flags.Intcon).GetField(tBlock.Name).GetValue(this); // gets the bit-index of the matching Option register
            Fields.ToggleSingleRegisterBit(Registers.INTCON, bit); // toggle the bit value

            //ToDo: Update UI
        }
        #endregion

        #region UI-Actions

        #endregion

    }
}
