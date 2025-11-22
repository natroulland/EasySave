using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.View.Languages;
using EasySave.View.MenuStrategies;
using Spectre.Console;

namespace EasySave.View.Menus;

public class HomeMenu : IMenuStrategy // Strategy pattern to display the home menu
{
    private Language _translations { get; set; }

    public HomeMenu(Language translations)
    {
        _translations = translations;
    }
    public string Display()
    {
        AnsiConsole.Clear();
        string userChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("menu"))
                .PageSize(10)
                .AddChoices(_translations.GetStringFromJson("menuExecuteJob"),
            _translations.GetStringFromJson("menuEditorJob"),
            _translations.GetStringFromJson("menuSettings"),
            _translations.GetStringFromJson("CryptoMenu"),
            _translations.GetStringFromJson("DecryptionMenu"),
            _translations.GetStringFromJson("menuExit"))
                );
        AnsiConsole.Clear();

        return userChoice;
    }
}
