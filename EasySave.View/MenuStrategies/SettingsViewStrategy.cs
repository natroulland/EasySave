using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.View.Languages;
using EasySave.ViewModel;
using Spectre.Console;

namespace EasySave.View.MenuStrategies;

internal class SettingsViewStrategy : IMenuStrategy
{
    private SettingsController settingsController;
    private Language _translations;

    public SettingsViewStrategy(Language translations)
    {
        settingsController = new();
        _translations = translations;
    }
    /// <summary>
    /// Displays Settings menu (Language, Reset to default, Back to main menu)
    /// </summary>
    /// <returns></returns>
    public string Display()
    {
        AnsiConsole.Clear();
        string userChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("settingsMenu"))
                .PageSize(10)
                .AddChoices(_translations.GetStringFromJson("settingsLanguage"), _translations.GetStringFromJson("settinglogtypechoice"), _translations.GetStringFromJson("settingsDefault"), _translations.GetStringFromJson("settingsBack"))
                );
        AnsiConsole.Clear();

        if (userChoice == _translations.GetStringFromJson("settingsBack")) return "";
        if (userChoice == _translations.GetStringFromJson("settingsLanguage"))
        {
            string language = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("languageChange"))
                .PageSize(10)
                .AddChoices("Francais", "English"));

            settingsController.ChangeLanguage(language);

            return "";
        }
        else if (userChoice == _translations.GetStringFromJson("settingsDefault"))
        {
            bool confirmation = AnsiConsole.Prompt(
                new TextPrompt<bool>(_translations.GetStringFromJson("resetConfigValidation"))
                    .AddChoice(true)
                    .AddChoice(false)
                    .WithConverter(choice => choice ? "y" : "n"));
            if (confirmation == true)
            {
                return settingsController.ResetDefaultSettings();
            }
            else
            {
                return "";
            }

        }
        else if (userChoice == _translations.GetStringFromJson("settinglogtypechoice"))
        {
            string type = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("settinglogtypechoice"))
                .PageSize(10)
                .AddChoices("json", "xml"));

            settingsController.ChangeLogType(type);

            return "";
        }




        else
        {
            AnsiConsole.Markup("[red]Error[/]");
            return "";
        }
    }
}
