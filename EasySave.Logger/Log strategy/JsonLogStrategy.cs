using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace EasySave.Logger.Log_strategy
{
    public class JsonLogStrategy : ILogStrategy
    {
        public void WriteLog(string filePath, string content)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            // Try to deserialize the content if it's already a valid JSON string
            object messageObject;
            try
            {
                messageObject = JsonSerializer.Deserialize<object>(content);
            }
            catch
            {
                messageObject = content; // If not a valid JSON, keep it as a raw string
            }

            // Create the final object to serialize
            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // More readable date format
                Message = messageObject
            };

            string jsonContent = JsonSerializer.Serialize(logEntry, options);

            using StreamWriter writer = new(filePath, true, new UTF8Encoding(false)); // UTF-8 without BOM
            writer.WriteLine(jsonContent);
        }
    }
}
