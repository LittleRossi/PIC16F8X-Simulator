using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PIC16F8X.DataModel
{
    class LSTFile
    {
        readonly ObservableCollection<SourceLine> sourceLines = new ObservableCollection<SourceLine>();




        readonly int[] linesWithCommands;
        int LastHighlightedLineIndex = 0;
        private LSTFile lSTFile;

        public LSTFile(string path)
        {
            int? lineIndexInPreview = null;
            int indexInProgrammStorage = 0;
            // set initial values in sourceLines
            for (int i = 0; i < 1024; i++)
                sourceLines.Add(new SourceLine("", "", "", "", false, lineIndexInPreview));

            // create List of 1024 "0" inital
            List<string> hexadecimalCommand = Enumerable.Repeat("#", 1024).ToList();


            //List<int> linesWithCommands = new List<int>(); //List of all indexes, that include a command
            List<int> linesWithCommands = Enumerable.Repeat(-1, 1024).ToList();

            string line;


            System.IO.StreamReader file = new System.IO.StreamReader(@path, System.Text.Encoding.UTF8);

            while ((line = file.ReadLine()) != null)
            {

                // Check if line contains a comment
                string comment = "";
                string[] lineCommentSplit = line.Split(";");
                
                if (lineCommentSplit.Length > 1)
                {
                    comment = lineCommentSplit[1];
                    line = lineCommentSplit[0];
                }

                //Check if line contains label
                string label;
                bool hasLabel = false;
                label = line.Substring(27).Split(" ")[0]; // labels always begin at the 28th char in a LST File

                if (label != "") hasLabel = true;


                // calculate the index of the line shown in preview
                //int index = System.Convert.ToInt32(line.Substring(20).Split(" ")[0]);

                //lineIndexInPreview = index;


                // Split the lines into every part and removing all dublicated whitespaces
                string[] lineComponents = line.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);



                

                // Check if the line contains a hexadecimal Command
                bool hasCommand = false;

                if (!char.IsWhiteSpace(line, 0)) //Lines with a Command always start with a character as first char
                {
                    // calculate the index of the command in the programm Storage
                    indexInProgrammStorage = System.Convert.ToInt32(lineComponents[0], 16);

                    hexadecimalCommand.Insert(indexInProgrammStorage, lineComponents[1]); // the actual hexadecimal command is in the second row
                    lineComponents = lineComponents.Skip(2).ToArray(); // Skipt the first two parts, because we already parsed them
                    linesWithCommands.Insert(indexInProgrammStorage, indexInProgrammStorage); // add the index of the line with a command
                    hasCommand = true;


                    //lineIndexInPreview = indexInProgrammStorage;
                }

                if (char.IsWhiteSpace(line, 0))
                {
                    lineIndexInPreview = System.Convert.ToInt32(lineComponents[0]);
                }


                // Fill LineNumber and text-command

                if (lineComponents.Length > 0)
                {
                    string lineNumber = lineComponents[0];
                    string textCommand = "";

                    if (lineComponents.Length > 1)
                    {
                        textCommand = string.Join(" ", lineComponents.Skip(hasLabel ? 2 : 1).ToArray()); // If line contains label we need to skip two lines, otherwise we only skip the linenumber
                    }

                    // Add the current line to the sourceLines
                    SourceLine currentLine = new SourceLine(lineNumber, label, textCommand, comment, hasCommand, indexInProgrammStorage);


                    if (hasCommand)
                    {
                        sourceLines.Insert((int)(lineIndexInPreview + indexInProgrammStorage), currentLine);
                    }
                    else
                    {
                        sourceLines.Insert((int)(lineIndexInPreview - 1), currentLine);
                    }


                }
            }
            file.Close();


            this.linesWithCommands = linesWithCommands.ToArray();

            DataModel.Fields.SetProgramm(hexadecimalCommand);
        }


        public void HighlightLine(int pc)
        {
            sourceLines[GetIndexInFileOfPCCommand(LastHighlightedLineIndex)].Active = false; //Set the current line to not active


            sourceLines[GetIndexInFileOfPCCommand(pc)].Active = true; // set the next line to active


            LastHighlightedLineIndex = pc;
        }


        private int GetIndexInFileOfPCCommand(int pc)
        {
            // calculate the index in File by using the Linenumber

            SourceLine line = sourceLines.FirstOrDefault(item => item.IndexInProgrammStorage == pc);

            return sourceLines.IndexOf(line);
        }
        
        public ObservableCollection<SourceLine> GetSourceLines()
        {
            return sourceLines;
        }

        public int GetSourceLineIndexFromPC(int pc)
        {
            // We need to check that, because there can be lines without commands
            if (pc < linesWithCommands.Max()) return linesWithCommands[pc]; 
            else return -1;
        }

        public bool LineHasBreakpoint(int pc)
        {
            return sourceLines[GetSourceLineIndexFromPC(pc)].Breakpoint; //Return if a line has a breakpoint
        }
    }

    

    class SourceLine : ObservableObject
    {
        public int? IndexInProgrammStorage { get; set; }
        public string LineNumber { get; set; }
        public string Label { get; set; }
        public string Command { get; set; }
        public string Comment { get; set; }

        public bool HasCommand { get; set; }

        private bool active;

        public bool Active // Indicates which line is currently active and gets executed
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

        public SourceLine (string lineNumber, string label, string command, string comment, bool hasCommand, int? indexInProgrammStorage)
        {
            IndexInProgrammStorage = indexInProgrammStorage;
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
