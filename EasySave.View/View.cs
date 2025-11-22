using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasySave.Core.Model;
using EasySave.View.Languages;
using EasySave.View.Menus;
using EasySave.View.MenuStrategies;
using EasySave.ViewModel;
using Newtonsoft.Json.Linq;


namespace EasySave.View;

public class View
{
    private string _language { get; set; }
    private Language _translations { get; set; }
    private Dictionary<string, IMenuStrategy> _strategy { get; set; }
    private HomeMenu _home {  get; set; }

    private JobExecutorViewStrategy jobExecutorViewStrategy;
    public View()
    {
        _language = GetLanguage();
        if(_language == "English")
        {
            _translations = new("../../../Languages/english.json");
        }
        else
        {
            _translations = new("../../../Languages/french.json");
        }
        jobExecutorViewStrategy = new(_translations);
        _strategy = new Dictionary<string, IMenuStrategy>
        {
            { _translations.GetStringFromJson("menuExecuteJob"), jobExecutorViewStrategy },
            { _translations.GetStringFromJson("menuEditorJob"), new JobEditorViewStrategy(_translations) },
            { _translations.GetStringFromJson("menuSettings"), new SettingsViewStrategy(_translations) },
            { _translations.GetStringFromJson("CryptoMenu"), new FileEncryptionViewStrategy(_translations) },
            { _translations.GetStringFromJson("DecryptionMenu"), new FileDecryptionViewStrategy(_translations) },
            { _translations.GetStringFromJson("menuExit"), new HomeMenu(_translations) }
        };

        _home = new(_translations);
        MainLoop();
    }

    public void MainLoop()
    {

        string userChoice = _home.Display();
        while (true)
        {
            if (userChoice == _translations.GetStringFromJson("menuExit"))
            {
                break;
            }
            else if (userChoice == "Reset Success")
            {
                ResetToDefault();
                userChoice = _home.Display();
            }
            else if (_strategy.TryGetValue(userChoice, out IMenuStrategy selectedMenu))
            {
                userChoice = selectedMenu.Display();
            }
            else
            {
                _language = GetLanguage();
                if (_language == "English")
                {
                    _translations = new("../../../Languages/english.json");
                }
                else
                {
                    _translations = new("../../../Languages/french.json");
                }
                _home = new(_translations);
                _strategy = new Dictionary<string, IMenuStrategy>
                {
                    { _translations.GetStringFromJson("menuExecuteJob"), new JobExecutorViewStrategy(_translations) },
                    { _translations.GetStringFromJson("menuEditorJob"), new JobEditorViewStrategy(_translations) },
                    { _translations.GetStringFromJson("menuSettings"), new SettingsViewStrategy(_translations) },
                    { _translations.GetStringFromJson("CryptoMenu"), new FileEncryptionViewStrategy(_translations) },
                    { _translations.GetStringFromJson("DecryptionMenu"), new FileDecryptionViewStrategy(_translations) },
                    { _translations.GetStringFromJson("menuExit"), new HomeMenu(_translations) }
                };
                userChoice = _home.Display();
            }
        }
    }
    public void ResetToDefault()
    {
        jobExecutorViewStrategy.GetJobsFromJson();
    }
    /// <summary>
    /// Get the language from the config file
    /// </summary>
    /// <returns></returns>
    public string GetLanguage()
    {
        string filePath = "config.json";
        if (!File.Exists(filePath))
        {
            using (File.Create("config.json")) { }

            JObject jsonObject = new()
            {
                ["Jobs"] = new JArray(),
                ["Language"] = "English",
                ["LogType"] = "json"
            };

            File.WriteAllText(filePath, jsonObject.ToString());
        }
        string json = File.ReadAllText(filePath);
        JObject jsonObj = JObject.Parse(json);

        return jsonObj["Language"].ToString();
    }
}