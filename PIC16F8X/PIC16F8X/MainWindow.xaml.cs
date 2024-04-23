using Microsoft.Win32;
using PIC16F8X.DataModel;
using PIC16F8X.ViewModel;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using PIC16F8X.helpfunctions;
using System.Collections.Specialized;
using System.Linq;
using System;

namespace PIC16F8X
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel View;
        private readonly Timer StepTimer;
        private LSTFile lSTFile;

        public MainWindow()
        {

            // DataContext for UI
            View = new MainWindowViewModel();
            DataContext = View;

            StepTimer = new Timer(View.SimSpeed); // Set the time between steps in the programm
            StepTimer.AutoReset = true;
            StepTimer.Elapsed += new ElapsedEventHandler(RunTimerEvent); // throws an event and calls the function every time the timer has elapsed

            //TODO


            // END TODO


            // initialize UI
            InitializeComponent();

            // Set initial Data
            Reset();
        }



        #region RunTime functions
        private void RunTimerEvent(object source, ElapsedEventArgs e)
        {
            //ToDo
        }
        #endregion



        #region User interaction functions
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

            UpdateUI();
        }

        private void TextBlock_OptionBitChange(object sender, MouseButtonEventArgs e)
        {
            TextBlock tBlock = (TextBlock)sender;
            int bit = (int)typeof(Flags.Option).GetField(tBlock.Name).GetValue(this); // gets the bit-index of the matching Option register
            Fields.ToggleSingleRegisterBit(Registers.OPTION, bit); // toggle the bit value

            UpdateUI();
        }

        private void TextBlock_IntconBitChange(object sender, MouseButtonEventArgs e)
        {
            TextBlock tBlock = (TextBlock)sender;
            int bit = (int)typeof(Flags.Intcon).GetField(tBlock.Name).GetValue(this); // gets the bit-index of the matching Option register
            Fields.ToggleSingleRegisterBit(Registers.INTCON, bit); // toggle the bit value

            UpdateUI();
        }

        private void TextBox_UpdateSource(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox box = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(box, prop);
                if (binding != null) { binding.UpdateSource(); }
            }
        }
        #endregion

        #region UI-Actions
        public void UpdateUI()
        {
            UpdateUIWithoutFileReg();
            // ToDO
        }

        public void UpdateUIWithoutFileReg()
        {
            // set the local model data to the ViewModel

            View.TrisA = new ObservableCollection<bool>(HelpFunctions.ConvertByteToBoolArray(Fields.GetRegister(Registers.TRISA)));
            View.TrisB = new ObservableCollection<bool>(HelpFunctions.ConvertByteToBoolArray(Fields.GetRegister(Registers.TRISB)));
            View.PortA = new ObservableCollection<bool>(HelpFunctions.ConvertByteToBoolArray(Fields.GetRegister(Registers.PORTA)));
            View.PortB = new ObservableCollection<bool>(HelpFunctions.ConvertByteToBoolArray(Fields.GetRegister(Registers.PORTB)));
            View.TrisA.CollectionChanged += new NotifyCollectionChangedEventHandler(TrisAChanged);
            View.TrisB.CollectionChanged += new NotifyCollectionChangedEventHandler(TrisBChanged);
            View.PortA.CollectionChanged += new NotifyCollectionChangedEventHandler(PortAChanged);
            View.PortB.CollectionChanged += new NotifyCollectionChangedEventHandler(PortBChanged);

            View.Status = new ObservableCollection<string>(HelpFunctions.ConvertByteToStringArray(Fields.GetRegister(Registers.STATUS)));
            View.Option = new ObservableCollection<string>(HelpFunctions.ConvertByteToStringArray(Fields.GetRegister(Registers.OPTION)));
            View.Intcon = new ObservableCollection<string>(HelpFunctions.ConvertByteToStringArray(Fields.GetRegister(Registers.INTCON)));

            View.StackDisplay = new ObservableCollection<string>(Fields.GetStack().Select(x => x.ToString("D4")).ToArray());

            View.SFRValues[0] = Fields.GetRegisterW().ToString("X2");
            View.SFRValues[1] = Fields.GetRegister(Registers.PCL).ToString("X2");
            View.SFRValues[2] = Fields.GetRegister(Registers.PCLATH).ToString("X2");
            View.SFRValues[3] = Fields.GetPc().ToString("D2");
            View.SFRValues[4] = Fields.GetRegister(Registers.STATUS).ToString("X2");
            View.SFRValues[5] = Fields.GetRegister(Registers.FSR).ToString("X2");
            View.SFRValues[6] = Fields.GetRegister(Registers.OPTION).ToString("X2");
            View.SFRValues[7] = Fields.GetRegister(Registers.TRM0).ToString("X2");
            View.SFRValues[8] = "1: " + Fields.GetPrePostScalerRatio();

            if (Fields.GetRegisterBit(Registers.OPTION, Flags.Option.PSA))
            {
                View.PrePostScalerText = "Postscaler"; // Postscaler is assigned to WatchDogTimer
            }
            else
            {
                View.PrePostScalerText = "Prescaler"; // Prescaler is assigned to TMR0
            }

            if (StepTimer.Enabled)
            {
                View.StartStopButtonText = "Stop"; // is programm is running, set the text for Button to Stop
            }
            else
            {
                View.StartStopButtonText = "Start"; // is programm is not running, set the text for Button to Start
            }

            if (Fields.GetPc() < Fields.GetProgramm().Count)
            {
                View.SFRValues[9] = Instructions.InstructionDecoder(Fields.GetProgramm()[Fields.GetPc()]).ToString(); // set the current command
            }
            else
            {
                View.SFRValues[9] = "NA";
            }

            View.Runtime = Fields.GetRuntime();
            View.Watchdog = Fields.GetWatchdog();

            if (lSTFile != null)
            {
                HighLightLine(Fields.GetPc());
            }
        }

        #endregion



        #region Control Buttons
        private void Button_StartStop_Click(object sender, RoutedEventArgs e)
        {
            //ToDo
        }

        private void Button_Step_Click(object sender, RoutedEventArgs e)
        {
            //ToDo
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            //ToDo
        }
        #endregion



        #region Checkbox ChangedHandlers
        private void TrisAChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Fields.SetRegister(Registers.TRISA, HelpFunctions.ConvertBoolArrayToByte(View.TrisA.ToArray<bool>()));
            UpdateUI();
        }
        private void TrisBChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Fields.SetRegister(Registers.TRISB, HelpFunctions.ConvertBoolArrayToByte(View.TrisB.ToArray<bool>()));
            UpdateUI();
        }
        private void PortAChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Fields.SetRegister(Registers.PORTA, HelpFunctions.ConvertBoolArrayToByte(View.PortA.ToArray<bool>()));
            UpdateUI();
        }
        private void PortBChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Fields.SetRegister(Registers.PORTB, HelpFunctions.ConvertBoolArrayToByte(View.PortB.ToArray<bool>()));
            UpdateUI();
        }
        #endregion


        #region HelpFunctions
        private void HighLightLine(int pcl)
        {
            try
            {
                lSTFile.HighlightLine(pcl);
                SourceDataGrid.ScrollIntoView(SourceDataGrid.Items[6]);
                SourceDataGrid.ScrollIntoView(SourceDataGrid.Items[lSTFile.GetSourceLineIndexFromPC(pcl)]);
            }
            catch (Exception)
            { }
        }
        #endregion

    }
}
