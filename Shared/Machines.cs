using System.ComponentModel.DataAnnotations;

namespace ArgosyUpdaterConsoleBLZ.Shared
{
    public class Machines
    {
        public class Root
        {
            public string? Odatacontext { get; set; }
            public List<Machine>? Value
            { get; set; }
        }

        public class Machine
        {
            public string? MachineName { get; set; }
            public string? IPadress { get; set; }
            public string? UserName { get; set; }
            [DataType(DataType.DateTime)]
            public DateTime LastSync { get; set; }
            public string? AppFolderVersions { get; set; }
            public string? LogChanges { get; set; }
            public string?  LogErrors { get; set; }
            public string? UpdaterTerminalError { get; set; }
            public string? ArgosyUpdaterVersion { get; set; }

        }
    }
}