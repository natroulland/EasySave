using EasySave.ViewModel;
using Spectre.Console;
using System;
using System.IO;
using System.Threading;
using EasySave.View.Languages;

namespace EasySave.View.MenuStrategies
{
    public class FileEncryptionViewStrategy : IMenuStrategy
    {
        private readonly FileEncryptionViewModel _viewModel;
        private readonly Language _translations;

        public FileEncryptionViewStrategy(Language translations)
        {
            _translations = translations;
            _viewModel = new FileEncryptionViewModel();
        }

        /// <summary>
        /// Displays the file encryption menu.
        /// </summary>
        public string Display()
        {
            var filePath = AnsiConsole.Ask<string>(_translations.GetStringFromJson("CryptoPickFile"));

            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("CryptoFileNotFoundError"));
                Thread.Sleep(1500);
                return "";
            }

            var password = AnsiConsole.Ask<string>(_translations.GetStringFromJson("CryptoPasswordQuery"));
            var confirmPassword = AnsiConsole.Ask<string>(_translations.GetStringFromJson("CryptoPasswordConfirmation"));

            if (password != confirmPassword)
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson(""));
                Thread.Sleep(1500);
                return "";
            }
            if(string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(password))
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorErrorFields"));
                Thread.Sleep(1500);
                return "";
            }
            bool success = _viewModel.EncryptFile(filePath, password);

            if (success)
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("CryptoSuccessful"));
                Thread.Sleep(1500);
            }
            else
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("CryptoFailed"));
                Thread.Sleep(1500);
            }

            Thread.Sleep(1500);
            return "";
        }
    }
}
