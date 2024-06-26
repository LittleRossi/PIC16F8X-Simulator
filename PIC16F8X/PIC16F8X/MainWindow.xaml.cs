﻿using Microsoft.Win32;
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
using System.ComponentModel;

namespace PIC16F8X
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel View;
        private readonly Timer StepTimer;
        private LSTFile lSTFile;
        private bool OutOfBoundMessageShown = false;
        private DateTime LastRegisterUpdate;

        public MainWindow()
        {

            // DataContext for UI
            View = new MainWindowViewModel();
            DataContext = View;

            StepTimer = new Timer(View.SimSpeed); // Set the time between steps in the programm
            StepTimer.AutoReset = true;
            StepTimer.Elapsed += new ElapsedEventHandler(RunTimerEvent); // throws an event and calls the function every time the timer has elapsed
            View.PropertyChanged += UpdateTimerInterval;

            // initialize UI
            InitializeComponent();

            // Set initial Data
            Reset();
        }


        #region RunTime functions
        private void RunTimerEvent(object source, ElapsedEventArgs e)
        {
            // Event that is thrown every time the StepTimer elapses

            ProgramStep(); //processes one Instruction

            Dispatcher.Invoke(() =>
            {
                if (DateTime.Now.Subtract(LastRegisterUpdate).TotalSeconds > 1)
                {
                    // Refresh Fileregister every 1 second
                    UpdateUI();
                    LastRegisterUpdate = DateTime.Now;
                }
                else
                {
                    // refresh UI but without FileRegister
                    UpdateUIWithoutFileReg();
                }

                // Check if Programm is out of range
                CheckProgrammOutOfRange();
            });
        }

        private void ProgramStep()
        {
            try
            {
                // Execute one Instruction
                InstructionProcessor.ProcessOnePCStep();
            }
            catch (IndexOutOfRangeException)
            {
                Stop();
                UpdateUI();

                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Index of used Register out of range.", "Index out of range"); // throw an error that index of register is out of range
                });
                return;
            }

            if (CheckIfPCIsOutOfRange())
            {
                if (!OutOfBoundMessageShown) StopAndUpdate();
            }
            else if (CheckIfBreakpointIsHit())
            {
                StopAndUpdate();
            }
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
                lSTFile = new LSTFile(dialog.FileName); // read and initialise programm in programmstorage as list of Commands

                SourceDataGrid.ItemsSource = lSTFile.GetSourceLines(); // set the data to the LST View
                SourceDataGrid.Columns[4].Width = 0; // Reset comment width
                SourceDataGrid.UpdateLayout();
                SourceDataGrid.Columns[4].Width = DataGridLength.Auto; // responsive width matching size of content
                UpdateUI();
            }
        }
        #endregion

        #region Control functions
        private void Start()
        {
            // Checks if the programm is initialized and starts the Steptimer to proceed Instructions
            if (Fields.IsProgrammInitialized())
            {
                FileRegister.IsReadOnly = true; // Set UI Readonly
                StepTimer.Start();
            }
        }
        private void Stop()
        {
            StepTimer.Stop();
            Dispatcher.Invoke(() =>
            {
                FileRegister.IsReadOnly = false; // Set UI editable
            });
        }
        private void StopAndUpdate()
        {
            Stop();
            Dispatcher.Invoke(() =>
            {
                UpdateUI();
            });
        }
        private void Reset()
        {
            Stop();
            Fields.ResetData();
            UpdateUI();
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

        #region Update-Actions
        public void UpdateUI()
        {
            UpdateUIWithoutFileReg();
            UpdateFileRegister();
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

        public void UpdateFileRegister()
        {
            string[,] data = new string[16, 16];

            int index = 0;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    data[i, j] = Fields.GetAllRegister()[index++].ToString("X2"); // "X2" converts to 2 Uppercase Hexadezimal
                }
            }

            View.FileRegisterData = data; // Set Data into View DataSource
        }

        private void UpdateTimerInterval(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SimSpeed")
            {
                StepTimer.Interval = View.SimSpeed; // set the Steptimer to the value of SimSpeed
            }
        }

        #endregion

        #region FileRegister
        private void FileRegister_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var edetingBox = e.EditingElement as TextBox;
            string newValue = edetingBox.Text;

            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (newValue.Length <= 2)
                {

                    try
                    {
                        byte b = (byte)int.Parse(newValue, System.Globalization.NumberStyles.HexNumber); //Convert input to hex
                        edetingBox.Text = b.ToString("X2");
                        int row = e.Row.GetIndex();
                        int column = e.Column.DisplayIndex;

                        Fields.SetRegister((byte)(row * 16 + column), b); // calculate index in array and set data
                        UpdateUI();

                        if (row > 0)
                        {
                            FileRegister.CurrentCell = new DataGridCellInfo(FileRegister.Items[row - 1], FileRegister.Columns[column]);
                        }
                    }
                    catch
                    {
                        e.Cancel = true;
                        (sender as DataGrid).CancelEdit(DataGridEditingUnit.Cell);
                        MessageBox.Show("Invalid hex value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    e.Cancel = true;
                    (sender as DataGrid).CancelEdit(DataGridEditingUnit.Cell);
                    MessageBox.Show("Only one hexbyte allowed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Control Buttons
        private void Button_StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (StepTimer.Enabled)
            {
                // if programm is currently running => Stop it
                Stop();
                UpdateUI();
            }
            else
            {
                // if programm is not running => start it
                Start();
            }
        }

        private void Button_Step_Click(object sender, RoutedEventArgs e)
        {
            if (Fields.IsProgrammInitialized())
            {
                ProgramStep(); // execute one Instruction
                UpdateUI();
                CheckIfPCIsOutOfRange();
            }
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            UpdateUI();
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

            // Direct set of PORTA to avoid output Latch
            Fields.DirectRegisterManipulation(0x5, HelpFunctions.ConvertBoolArrayToByte(View.PortA.ToArray<bool>()));
            Fields.SetDataLatchA(HelpFunctions.ConvertBoolArrayToByte(View.PortA.ToArray<bool>()));

            UpdateUI();
        }
        private void PortBChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Direct set of PORTB to avoid output Latch
            Fields.DirectRegisterManipulation(0x6, HelpFunctions.ConvertBoolArrayToByte(View.PortB.ToArray<bool>()));
            Fields.SetDataLatchA(HelpFunctions.ConvertBoolArrayToByte(View.PortB.ToArray<bool>()));
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
                SourceDataGrid.ScrollIntoView(SourceDataGrid.Items[lSTFile.GetIndexInFileOfPCCommand(pcl)]);
            }
            catch (Exception)
            { }
        }
        private bool CheckIfPCIsOutOfRange()
        {
            return (Fields.GetPc() >= Fields.GetProgramm().Count);
        }
        public bool CheckIfBreakpointIsHit()
        {
            return lSTFile.LineHasBreakpoint(Fields.GetPc());
        }
        private void CheckProgrammOutOfRange()
        {
            if (CheckIfPCIsOutOfRange())
            {
                if (!OutOfBoundMessageShown)
                {
                    // if message is not shown, show it
                    OutOfBoundMessageShown = true;
                    MessageBox.Show("PC out of Programm range. Please end programm with endless loop", "programm out of range", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                OutOfBoundMessageShown = false;
            }
        }
        #endregion
    }
}
