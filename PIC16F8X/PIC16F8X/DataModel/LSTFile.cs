using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;

namespace PIC16F8X.DataModel
{
    class LSTFile
    {
        readonly ObservableCollection<SourceLine> sourceLines = new ObservableCollection<SourceLine>();

        public LSTFile(string path)
        {
            
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
