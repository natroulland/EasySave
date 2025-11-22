using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.Model;

namespace EasySave.ViewModel;

public class SettingsController
{
    SettingsModel settingsModel;   
    public SettingsController() 
    {
        settingsModel = new();
    }
    public string ResetDefaultSettings()
    {
        return settingsModel.ResetToDefault();
    }
    public void ChangeLogType(string type)
    {
        settingsModel.ChangeLogType(type);
    }

    public void ChangeLanguage(string language)
    {
        settingsModel.ChangeLanguage(language);
    }

    /// <summary>
    /// Changes the file extensions in the config file
    /// </summary>
    /// <param name="extensions">List of extensions to set</param>
    public void ChangeExtensions(List<string> extensions)
    {
        settingsModel.ChangeExtensions(extensions);
    }

    public void ChangeBusinessSoftware(string businessSoftware)
    {
        settingsModel.ChangeBusinessSoftware(businessSoftware);
    }
}
