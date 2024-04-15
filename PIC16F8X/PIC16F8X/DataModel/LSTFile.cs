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

    }
}
