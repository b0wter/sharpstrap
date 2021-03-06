using System;
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
        IEnumerable<LogEntry> LoadOldLog(string logFilename);
        void SaveNewLog(string logFilename, IEnumerable<LogEntry> entries);
    }

    /// <summary>
    /// Reads and writes status log files to the local filesystem.
    /// </summary>
    public class FileBootstrapStatusLogger : IBootstrapStatusLogger
    {
        public IEnumerable<LogEntry> LoadOldLog(string logFilename)
        {
            if (System.IO.File.Exists(logFilename) == false)
                return new LogEntry[0];
            
            var entries = new List<LogEntry>();
            using (var reader = File.OpenText(logFilename))
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

        /// <summary>
        /// Writes a log file to the local file system. Contains the state of each package.
        /// </summary>
        /// <example>
        /// The resulting log file looks like this:
        ///
        /// [Solved]
        /// Package #1
        /// Package #2
        /// ...
        ///
        /// [Failed]
        /// Package #5
        /// Package #6
        /// ...
        ///
        /// </example>
        /// <param name="logFilename"></param>
        /// <param name="entries"></param>
        public void SaveNewLog(string logFilename, IEnumerable<LogEntry> entries)
        {
            var isFirst = true;
            using (var writer = File.CreateText(logFilename))
            {
                var groupedEntries = entries.GroupBy(x => x.Status);
                foreach (var group in groupedEntries)
                {
                    var header = $"{(isFirst ? string.Empty : Environment.NewLine)}[{group.Key}]";
                    isFirst = false;
                    writer.WriteLine(header);
                    foreach(var value in group)
                        writer.WriteLine((value.Name));
                }
            }
        }
    }
}