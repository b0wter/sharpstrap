using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

namespace SharpStrap.Helpers
{
    /// <summary>
    /// Abstract definition of a status log entry.
    /// </summary>
    public class LogEntry
    {
        public string Name { get; set; }
        public string Status { get; set; }
    }
    
    /// <summary>
    /// Reads and persists status of the bootstrap operation.
    /// </summary>
    public interface IBootstrapStatusLogger
    {
        IEnumerable<LogEntry> LoadOldLog();
        void SaveNewLog(IEnumerable<LogEntry> entries);
    }

    /// <summary>
    /// Reads and writes status log files to the local filesystem.
    /// </summary>
    public class FileBootstrapStatusLogger : IBootstrapStatusLogger
    {
        private readonly string logFilename;
        
        public FileBootstrapStatusLogger(string logFilename)
        {
            this.logFilename = logFilename;
        }
        
        public IEnumerable<LogEntry> LoadOldLog()
        {
            var entries = new List<LogEntry>();
            using (var reader = File.OpenText(this.logFilename))
            {
                var line = string.Empty;
                var currentStatus = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    if(string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                        currentStatus = line.Substring(1, line.Length - 2);
                    else
                        entries.Add(new LogEntry() { Name = line, Status = currentStatus });
                }
            }

            return entries;
        }

        public void SaveNewLog(IEnumerable<LogEntry> entries)
        {
            using (var writer = File.CreateText(this.logFilename))
            {
                var groupedEntries = entries.GroupBy(x => x.Status);
                foreach (var group in groupedEntries)
                {
                    writer.WriteLine(group.Key);
                    foreach(var value in group)
                        writer.WriteLine((value.Name));
                }
            }
        }
    }
}