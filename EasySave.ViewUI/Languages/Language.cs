using Newtonsoft.Json;
using System.IO;

namespace EasySave.ViewUI.Languages;

public class Language
{
    private string filePath;

    public Language(string filePath)
    {
        this.filePath = filePath;
    }

    /// <summary>
    /// Get the string associated with a key in language json file
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetStringFromJson(string key)
    {
        string json = File.ReadAllText(filePath);
        var jsonObject = JsonConvert.DeserializeObject<dynamic>(json);

        string value = jsonObject[key];

        if (value == null)
        {
            return "";
        }
        return value;
    }
}
