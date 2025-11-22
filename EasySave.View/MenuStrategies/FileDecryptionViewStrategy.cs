using EasySave.ViewModel;
using Spectre.Console;
using System;
using System.Threading;
using EasySave.View.Languages;

namespace EasySave.View.MenuStrategies
{
    public class FileDecryptionViewStrategy : IMenuStrategy
    {
        private readonly FileDecryptionViewModel _viewModel;
        private readonly Language _translations;

        public FileDecryptionViewStrategy(Language translations)
        {
            _translations = translations;
            _viewModel = new FileDecryptionViewModel();
        }

        public string Display()
        {
            var path = AnsiConsole.Ask<string>(_translations.GetStringFromJson("DecryptionPickFile"));

            if (string.IsNullOrEmpty(path))
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("DecryptionFileNotFoundError"));
                Thread.Sleep(1500);
                return "";
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("DecryptionFileNotFoundError"));
                Thread.Sleep(1500);
                return "";
            }

            string password = null;
            if (File.Exists(path))
            {
                password = AnsiConsole.Ask<string>(_translations.GetStringFromJson("DecryptionPasswordQuery"));
                if (string.IsNullOrEmpty(password))
                {
                    AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorErrorFields"));
                    Thread.Sleep(1500);
                    return "";
                }
            }

            

            Thread.Sleep(1500);
            return "";
        }
    }



}
