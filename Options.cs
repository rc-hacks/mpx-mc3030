using Mpx;
using System;

namespace MpxMc3030
{
    public class Options : CommandLineOptions
    {
        public bool Validate()
        {
            if (Port != null && Id)
                return true;

            if (Port != null && Dump != null)
                return true;

            if (Port != null && Diff != null)
                return true;

            if (Port != null && Load != null)
                return true;

            if (Port != null && Save != null)
                return true;

            return false;
        }

        [CommandLineOption(Arguments = "COMx", Description = "Specifies the COM port to be used")]
        public string Port = null;

        [CommandLineOption(Description = "Print the transmitter ID block")]
        public bool Id = false;

        [CommandLineOption(Arguments = "block", Description = "Dumps the specified block to the console")]
        public string Dump = null;

        [CommandLineOption(Arguments = "block", Description = "Repeatatly load the specified block and print changes to the console")]
        public string Diff = null;

        [CommandLineOption(Arguments = "filename", Description = "Save data from the MC3030 to the specified file")]
        public string Save = null;

        [CommandLineOption(Arguments = "filename", Description = "Load the specified file and transfer the data to the MC3030")]
        public string Load = null;

        [CommandLineOption(Arguments = "memory", Description = "Specifies the model memory to be loaded/saved")]
        public string Memory = null;

        [CommandLineOption(Arguments = "block", Description = "Specifies the block to be saved")]
        public string Block = null;

        [CommandLineOption(Alias = "first", Arguments = "block", Description = "Specifies the first block to be saved")]
        public string FirstBlock = null;

        [CommandLineOption(Alias = "last", Arguments = "block", Description = "Specifies the last block to be saved")]
        public string LastBlock = null;

        [CommandLineOption(Alias = "?", Category = "Miscellaneous", Description = "Display full help")]
        public bool Help = false;
    }
}
