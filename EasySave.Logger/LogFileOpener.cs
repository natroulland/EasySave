using EasySave.Logger.Log_strategy;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace EasySave.Logger
{
    internal class LogFileOpener
    {
        private static readonly Lazy<LogFileOpener> _instance =
            new(() => new LogFileOpener());

        private readonly string logDirectory = "logs";
        private ILogStrategy logStrategy = new JsonLogStrategy();
        private string currentExtension;

        private LogFileOpener()
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
            string filePath = "config.json";
            string json = File.ReadAllText(filePath);
            JObject jsonObj = JObject.Parse(json);
            string type = jsonObj["LogType"].ToString();
            TypeChanger(type);
        }

        public void TypeChanger(string type)
        {
            if (type == "json")
            {
                SetLogStrategy(new JsonLogStrategy());
                currentExtension = "json";
            }
            else
            {
                SetLogStrategy(new XmlLogStrategy());
                currentExtension = "xml";
            }
        }



        public static LogFileOpener Instance => _instance.Value;
        /// <summary>
        /// Writes the content to the log file
        /// </summary>
        /// <param name="content"></param>

        public void SetLogStrategy(ILogStrategy strategy)
        {
            if (strategy != null && strategy.GetType() != logStrategy.GetType())
            {
                string logFilePath = EnsureDailyLogFileExists();
                using (StreamWriter writer = new(logFilePath, true, System.Text.Encoding.UTF8))
                {
                    logStrategy = strategy;
                    currentExtension = logStrategy is XmlLogStrategy ? "xml" : "json";
                }
            }
        }


        /// <summary>
        /// Ensure that a log file exists for the current day
        /// </summary>
        public void WriteToFile(string content)
        {
            string filePath = "config.json";
            string json = File.ReadAllText(filePath);
            JObject jsonObj = JObject.Parse(json);
            string type = jsonObj["LogType"].ToString();
            TypeChanger(type);
            string logFilePath = EnsureDailyLogFileExists();
            logStrategy.WriteLog(logFilePath, content);
        }

        private string EnsureDailyLogFileExists()
        {
            string logFileName = $"EasySave-{DateTime.Now:yyyy-MM-dd}.{currentExtension ?? "json"}";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            if (!File.Exists(logFilePath))
            {
                if (currentExtension == "xml")
                {
                    File.WriteAllText(logFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Logs>\n</Logs>");
                }
                else
                {
                    File.WriteAllText(logFilePath, "[]"); // JSON starts as an empty array
                }
            }

            return logFilePath;
        }

    }
}
