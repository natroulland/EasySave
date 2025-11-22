using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace EasySave.Logger.Log_strategy
{
    public class XmlLogStrategy : ILogStrategy
    {
        public void WriteLog(string filePath, string content)
        {
            XmlDocument doc = new();

            // load entry 
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                doc.Load(filePath);
            }
            else
            {
                XmlDeclaration xmlDecl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(xmlDecl);
                XmlElement root = doc.CreateElement("Logs");
                doc.AppendChild(root);
            }

            // Deserialize json
            Dictionary<string, string> logData;
            try
            {
                logData = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            }
            catch
            {
                logData = new Dictionary<string, string> { { "Message", content } };
            }

            // XML Creation
            XmlElement logEntry = doc.CreateElement("LogEntry");

            foreach (var pair in logData)
            {
                XmlElement element = doc.CreateElement(pair.Key);
                element.InnerText = pair.Value;
                logEntry.AppendChild(element);
            }

            doc.DocumentElement?.AppendChild(logEntry);
            doc.Save(filePath);
        }
    }
}
