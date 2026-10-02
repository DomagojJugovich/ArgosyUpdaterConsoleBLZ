using System.ComponentModel.DataAnnotations;

namespace ArgosyUpdaterConsoleBLZ.Shared
{
    public class Machines
    {

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

        // Full log text for one machine, loaded on demand when the grid detail row is expanded.
        public class MachineLogs
        {
            public string? LogChanges { get; set; }
            public string? LogErrors { get; set; }
        }

        // Sidebar counters.
        public class MachineStats
        {
            public int Total { get; set; }
            // Machines whose ArgosyUpdaterVersion is lower than LatestVersion (or missing/unparseable).
            public int WaitingForUpgrade { get; set; }
            // Machines with non-empty LogErrors.
            public int InError { get; set; }
            // Highest ArgosyUpdaterVersion found in the database.
            public string? LatestVersion { get; set; }
        }
    }
}