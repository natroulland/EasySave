using EasySave.Logger.Log_strategy;
using Newtonsoft.Json.Linq;
using EasySave.Logger;

namespace EasySave.Core.Model;

public class SettingsModel
{
    /// <summary>
    /// Resets the config file to default
    /// </summary>
    /// <returns></returns>
    public string ResetToDefault()
    {
        if (File.Exists("./config.json"))
        {
            try
            {
                string filePath = "config.json";
                string json = File.ReadAllText(filePath);
                JObject jsonObj = JObject.Parse(json);

                jsonObj["Language"] = "English";
                jsonObj["LogType"] = "json";
                jsonObj["Extensions"] = new JArray();
                jsonObj["ExtensionsPriority"] = new JArray();
                jsonObj["BusinessSoftware"] = "";
                jsonObj["MaxFileSizeKo"] = 10000;
                File.WriteAllText(filePath, jsonObj.ToString());
                return "Reset Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        else
        {
            try
            {
                File.Create("./config.json");
                string filePath = "config.json";
                string json = File.ReadAllText(filePath);
                JObject jsonObj = JObject.Parse(json);


                jsonObj["Jobs"] = new JArray();
                jsonObj["Language"] = "English";
                jsonObj["LogType"] = "json";
                jsonObj["Extensions"] = new JArray();
                jsonObj["ExtensionsPriority"] = new JArray();
                jsonObj["BusinessSoftware"] = "";
                jsonObj["MaxFileSizeKo"] = 10000;
                File.WriteAllText(filePath, jsonObj.ToString());

                return "Reset Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
    /// <summary>
    /// Changes the language in the config file
    /// </summary>
    /// <param name="language"></param>
    public void ChangeLanguage(string language)
    {
        string filePath = "config.json";
        string json = File.ReadAllText(filePath);
        JObject jsonObj = JObject.Parse(json);

        jsonObj["Language"] = language;
        File.WriteAllText(filePath, jsonObj.ToString());
    }


    public void ChangeLogType(string logsFormat)
    {
        string filePath = "config.json";
        string json = File.ReadAllText(filePath);
        JObject jsonObj = JObject.Parse(json);

        jsonObj["LogType"] = logsFormat;
        File.WriteAllText(filePath, jsonObj.ToString());
    }

    public void ChangeBusinessSoftware(string businessSoftware)
    {
        string filePath = "config.json";
        string json = File.ReadAllText(filePath);
        JObject jsonObj = JObject.Parse(json);

        jsonObj["BusinessSoftware"] = businessSoftware;
        File.WriteAllText(filePath, jsonObj.ToString());
    }

    public void ChangeExtensions(List<string> extensions)
    {
        string filePath = "./config.json";
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                JObject jsonObj = JObject.Parse(json);

                JArray extensionsArray = new JArray(extensions);
                jsonObj["Extensions"] = extensionsArray;

                File.WriteAllText(filePath, jsonObj.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing file extensions: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Config file does not exist.");
        }
    }

    public void ChangeExtensionsPriority(List<string> extensions)
    {
        string filePath = "./config.json";
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                JObject jsonObj = JObject.Parse(json);

                JArray extensionsArray = new JArray(extensions);
                jsonObj["ExtensionsPriority"] = extensionsArray;

                File.WriteAllText(filePath, jsonObj.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing file extensions: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Config file does not exist.");
        }
    }
    public void ChangeMaxFileSize(int maxFileSizeKo)
    {
        string filePath = "config.json";
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                JObject jsonObj = JObject.Parse(json);

                jsonObj["MaxFileSizeKo"] = maxFileSizeKo;
                File.WriteAllText(filePath, jsonObj.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing max file size: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Config file does not exist.");
        }
    }
}

