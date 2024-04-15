using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PIC16F8X.DataModel
{
    class LSTFile
    {
        readonly ObservableCollection<SourceLine> sourceLines = new ObservableCollection<SourceLine>();
        readonly int[] linesWithCommands;
        int LastHighlightedLineIndex = 0;

        public LSTFile(string path)
        {
            List<string> commands = new List<string>();
            List<int> linesWithCommands = new List<int>();

            string line;
            int lineCounter = 0;


            System.IO.StreamReader file = new System.IO.StreamReader(@path, System.Text.Encoding.UTF8);

            while ((line = file.ReadLine()) != null)
            {
                // Check if line contains a command


                // check if line contains a label


                // remove all dublicate white spaces

                // check if line contains hex decimal command


                // Fill linenumber and command variable

                // add line to list of SourceLines
            }





            // Neues LSTFile wird erzeugt, wenn LST Datei in UI ausgewählt wird
            // Dadurch wird dieses dann direkt gesetzt in Programmspeicher und initialisiert


            // LST File einlesen
            // Filtern nach relevanten Zeilen
            // prüfen ob Label vorhanden
            // prüfen nach hexadecimal Command => In liste von String
            // Aus String liste der Commands SetProgramm() aufrufen und dort die Liste übergeben um das Programm zu initialisieren
        }
    }

    class SourceLine : ObservableObject
    {
        public string LineNumber { get; set; }
        public string Label { get; set; }
        public string Command { get; set; }
        public string Comment { get; set; }

        public bool HasCommand { get; set; }

        private bool active;

        public bool Active // public property to check if line is active and send notification by setting it active to execute the line
        {
            get { return active; }
            set { SetAndNotify(ref active, value, () => Active); }
        }

        private bool breakpoint;
        public bool Breakpoint // public property to check for breakpoints and send notifications by setting a breakpoint
        {
            get { return breakpoint; }
            set { SetAndNotify(ref breakpoint, value, () => Breakpoint); }
        }

        public SourceLine (string lineNumber, string label, string command, string comment, bool hasCommand)
        {
            breakpoint = false; //default we dont have a breakpoint by reading the file, breakpoints get set in the UI
            LineNumber = lineNumber;
            Label = label;
            Command = command;
            Comment = comment;
            HasCommand = hasCommand;
            active = false; //default all lines are not active, we set them to active when we execute them

        }
    }
}
