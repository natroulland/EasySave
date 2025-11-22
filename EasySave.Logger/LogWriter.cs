using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EasySave.Logger
{
    public class LogWriter : Observer
    {
        /// <summary>
        /// Update the log file with the new log entry data.
        /// </summary>
        /// <param name="data"></param>
        public override void Update(Dictionary<string, string> data)
        {
            if (data == null || !data.ContainsKey("subject") || data["subject"] != "log")
                return;

            
            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Name = data.GetValueOrDefault("Name", "Unknown"),
                FileSource = data.GetValueOrDefault("FileSource", "Unknown"),
                FileTarget = data.GetValueOrDefault("FileTarget", "Unknown"),
                FileSize = data.GetValueOrDefault("FileSize", "0"),
                FileTransferTime = data.GetValueOrDefault("FileTransferTime", "0"),
                CryptTime = data.GetValueOrDefault("CryptTime", "0"),
                Time = data.GetValueOrDefault("Time", DateTime.Now.ToString("HH:mm:ss"))
            };

            string jsonLog = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                LogFileOpener.Instance.WriteToFile(jsonLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log: {ex.Message}");
            }
        }
    }
}
