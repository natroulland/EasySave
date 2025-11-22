using System.Windows;
using System.Windows.Controls;
using EasySave.ViewModel;
using Microsoft.Win32;
using System.IO;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using EasySave.ViewUI.Languages;

namespace EasySave.ViewUI.UserControls.DecryptionWindow
{
    public partial class DecryptionWindowMenu : UserControl
    {
        private readonly FileDecryptionViewModel _viewModel;
        private Language _translations;

        public DecryptionWindowMenu(Language translations)
        {
            InitializeComponent();
            _viewModel = new FileDecryptionViewModel();
            _translations = translations;
            TranslateComponents();
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    PathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            string path = PathTextBox.Text;

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show(
                    _translations.GetStringFromJson("Error_NoFolderSelected"),
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            if (!Directory.Exists(path))
            {
                MessageBox.Show(
                    _translations.GetStringFromJson("Error_InvalidPath"),
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            int result = _viewModel.DecryptDirectory(path);

            if (result >= 0)
            {
                MessageBox.Show(
                    _translations.GetStringFromJson("Success_Decryption"),
                    _translations.GetStringFromJson("Success_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
           
            }
            else
            {
                string errorMessage = result switch
                {
                    -1 => _translations.GetStringFromJson("Error_InvalidFields"),
                    -2 => _translations.GetStringFromJson("Error_IncorrectPassword"),
                    -3 => _translations.GetStringFromJson("Error_DecryptionFailed"),
                    -4 => _translations.GetStringFromJson("Error_InvalidPathCode"),
                    -5 => _translations.GetStringFromJson("Error_JobNameNotFound"),
                    _ => _translations.GetStringFromJson("Error_DecryptionFailed")
                };

                MessageBox.Show(
                    errorMessage,
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        public void TranslateComponents()
        {
            SelectFolderToDecrypt.Text = _translations.GetStringFromJson("SelectFolderToDecrypt");
            ChooseFolderButton.Content = _translations.GetStringFromJson("ChooseFolderButton");
            SelectedPathLabel.Text = _translations.GetStringFromJson("SelectedPathLabel");
            PasswordLabel.Text = _translations.GetStringFromJson("PasswordLabel");
            DecryptButton.Content = _translations.GetStringFromJson("DecryptButton");
        }
    }
}
