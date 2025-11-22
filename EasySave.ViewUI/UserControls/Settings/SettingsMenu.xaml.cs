using EasySave.ViewModel;
using EasySave.ViewUI.Languages;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasySave.ViewUI.UserControls.Settings
{
 
    public partial class SettingsMenu : UserControl
    {
        private SettingsController settingsController;
        private Language _translations;

        public SettingsMenu(Language translations)
        {
            InitializeComponent();
            settingsController = new();

            _translations = translations;
            TranslateComponents();
            Update_SettingsMenu();
        }

        public void Dispose()
        {

        }

        private void Reset_settings_button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                    _translations.GetStringFromJson("Confirmation_Message"),
                    _translations.GetStringFromJson("Confirmation_Title"),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                settingsController.ResetDefaultSettings();
                Update_SettingsMenu();
            }
        }

        private string GetLogFormat()
        {
            string filePath = "config.json";
            string json = File.ReadAllText(filePath);
            JObject jsonObj = JObject.Parse(json);

            return jsonObj["LogType"].ToString();
        }

        private void save_settings_button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                    _translations.GetStringFromJson("Confirmation_Message"),
                    _translations.GetStringFromJson("Confirmation_Title"),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                List<RadioButton> logsRadioButtons = GetRadioButtons(logs_radios);
                string logsFormat = "";
                foreach (RadioButton radioButton in logsRadioButtons)
                {
                    if (radioButton.IsChecked == true)
                    {
                        logsFormat = radioButton.Content.ToString();
                    }
                }
                settingsController.ChangeLogType(logsFormat);

                List<RadioButton> languageRadioButtons = GetRadioButtons(languages_radios);
                string newLanguage = "";
                foreach (RadioButton radioButton in languageRadioButtons)
                {
                    if (radioButton.IsChecked == true)
                    {
                        newLanguage = radioButton.Content.ToString();
                    }
                }
                settingsController.ChangeLanguage(newLanguage);

                // Valider les extensions
                if (!ValidateExtensions(Extensions_Textbox.Text))
                {
                    return;
                }

                List<string> extensions = Extensions_Textbox.Text.Split(',').ToList();
                List<string> extensionsTrimmed = extensions.Select(ext => ext.Trim()).ToList();
                settingsController.ChangeExtensions(extensionsTrimmed);

                // Valider les extensions prioritaires
                if (!ValidateExtensions(Extensions_Priority_textbox.Text))
                {
                    return;
                }

                List<string> extensionsPriority = Extensions_Priority_textbox.Text.Split(',').ToList();
                List<string> extensionsTrimmedPriority = extensionsPriority.Select(ext => ext.Trim()).ToList();
                settingsController.ChangeExtensionsPriority(extensionsTrimmedPriority);

                settingsController.ChangeBusinessSoftware(BusinessSoftware_Textbox.Text);
                MaxFileSize_TextBox.Text = ((int)MaxFileSize_Slider.Value).ToString();
                settingsController.ChangeMaxFileSize((int)MaxFileSize_Slider.Value);
                cancel_button.IsEnabled = false;
                save_settings_button.IsEnabled = false;
            }
        }

        


        private List<RadioButton> GetRadioButtons(Panel panel)
        {
            List<RadioButton> radioButtons = new List<RadioButton>();

            foreach (var child in panel.Children)
            {
                if (child is RadioButton radioButton)
                {
                    radioButtons.Add(radioButton);
                }
            }

            return radioButtons;
        }

        public string GetLanguage()
        {
            string filePath = "config.json";
            if (!File.Exists(filePath))
            {
                using (File.Create("config.json")) { }

                JObject jsonObject = new()
                {
                    ["Jobs"] = new JArray(),
                    ["Language"] = "English"
                };

                File.WriteAllText(filePath, jsonObject.ToString());
            }
            string json = File.ReadAllText(filePath);
            JObject jsonObj = JObject.Parse(json);

            return jsonObj["Language"].ToString();
        }

        public string GetBusinessSoftware()
        {
            string filePath = "config.json";
            string json = File.ReadAllText(filePath);
            JObject jsonObj = JObject.Parse(json);

            return jsonObj["BusinessSoftware"].ToString();
        }

        private void english_language_radio_Checked(object sender, RoutedEventArgs e)
        {
            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        private void french_language_radio_Checked(object sender, RoutedEventArgs e)
        {
            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        private void json_logs_radio_Checked(object sender, RoutedEventArgs e)
        {
            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        private void xml_logs_radio_Checked(object sender, RoutedEventArgs e)
        {
            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            Update_SettingsMenu();

            cancel_button.IsEnabled = false;
            save_settings_button.IsEnabled = false;
        }

        private string GetExtensions()
        {
            string json = File.ReadAllText("./config.json");

            if (string.IsNullOrWhiteSpace(json))
            {
                return "";
            }

            JObject jsonObj = JObject.Parse(json);

            var listExtensions = jsonObj["Extensions"].ToObject<List<string>>();
            string extensionsFormatted = "";

            foreach (var extension in listExtensions)
            {
                if (listExtensions.IndexOf(extension) == listExtensions.Count - 1)
                {
                    extensionsFormatted += extension.ToString();
                }
                else
                    extensionsFormatted += extension.ToString() + ", ";
            }

            return extensionsFormatted;
        }

        private string GetExtensionsPriority()
        {
            string json = File.ReadAllText("./config.json");

            if (string.IsNullOrWhiteSpace(json))
            {
                return "";
            }

            JObject jsonObj = JObject.Parse(json);

            var listExtensions = jsonObj["ExtensionsPriority"].ToObject<List<string>>();
            string extensionsFormatted = "";

            foreach (var extension in listExtensions)
            {
                if (listExtensions.IndexOf(extension) == listExtensions.Count - 1)
                {
                    extensionsFormatted += extension.ToString();
                }
                else
                    extensionsFormatted += extension.ToString() + ", ";
            }

            return extensionsFormatted;
        }

        private void Extensions_Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        private void Update_SettingsMenu()
        {
            string logFormat = GetLogFormat();
            if (logFormat.ToLower() == "json")
            {
                json_logs_radio.IsChecked = true;
            }
            else
            {
                xml_logs_radio.IsChecked = true;
            }

            string language = GetLanguage();
            if (language == "Français")
            {
                french_language_radio.IsChecked = true;
            }
            else
            {
                english_language_radio.IsChecked = true;
            }
            Extensions_Textbox.Text = GetExtensions();
            Extensions_Priority_textbox.Text = GetExtensionsPriority();
            BusinessSoftware_Textbox.Text = GetBusinessSoftware();
            string filePath = "config.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                JObject jsonObj = JObject.Parse(json);
                int maxFileSize = jsonObj["MaxFileSizeKo"].Value<int>();

               
                if (MaxFileSize_Slider != null)
                {
                    MaxFileSize_Slider.Value = maxFileSize;
                }
                if (MaxFileSize_TextBox != null)
                {
                    MaxFileSize_TextBox.Text = maxFileSize.ToString();
                }
            }
        }
        private void Extensions_Priority_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        private void BusinessSoftware_button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = _translations.GetStringFromJson("OpenFileDialog_Filter");
                dialog.Title = _translations.GetStringFromJson("OpenFileDialog_Title");

                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string fullPath = dialog.FileName;
                    string fileName = System.IO.Path.GetFileName(fullPath);
                    BusinessSoftware_Textbox.Text = fileName;
                }
            }
            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        public void TranslateComponents()
        {
            Reset_settings_button.Content = _translations.GetStringFromJson("Reset_settings_button");
            json_logs_radio.Content = _translations.GetStringFromJson("json_logs_radio");
            xml_logs_radio.Content = _translations.GetStringFromJson("xml_logs_radio");
            BusinessSoftware_button.Content = _translations.GetStringFromJson("BusinessSoftware_button");
            save_settings_button.Content = _translations.GetStringFromJson("Save_Changes_Button");
            cancel_button.Content = _translations.GetStringFromJson("Cancel_Button");

            
            Logs_file_format_TextBlock.Text = _translations.GetStringFromJson("Logs_file_format_TextBlock");
            Language_TextBlock.Text = _translations.GetStringFromJson("Language_TextBlock");
            Extensions_TextBlock.Text = _translations.GetStringFromJson("Extensions_TextBlock");
            Extensions_Description_TextBlock.Text = _translations.GetStringFromJson("Extensions_Description_TextBlock");
            BusinessSoftware_TextBlock.Text = _translations.GetStringFromJson("BusinessSoftware_TextBlock");
            MaxFileSize_TextBlock.Text = _translations.GetStringFromJson("MaxFileSize_TextBlock");
            Extensions_Priority_Description_TextBlock_Copy.Text = _translations.GetStringFromJson("Extensions_Description_TextBlock");
            Extensions_Priority_TextBlock.Text = _translations.GetStringFromJson("Extensions_Priority_TextBlock");
        }

        

        private void MaxFileSize_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
            if (MaxFileSize_TextBox != null)
            {
                MaxFileSize_TextBox.Text = ((int)MaxFileSize_Slider.Value).ToString();
            }

            cancel_button.IsEnabled = true;
            save_settings_button.IsEnabled = true;
        }

        private void MaxFileSize_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(MaxFileSize_TextBox.Text, out int value))
            {
                
                if (MaxFileSize_Slider != null)
                {
                    MaxFileSize_Slider.Value = value;
                }

                cancel_button.IsEnabled = true;
                save_settings_button.IsEnabled = true;
            }
        }

        private void french_language_radio_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void json_logs_radio_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void xml_logs_radio_Checked_1(object sender, RoutedEventArgs e)
        {

        }
        private void Extensions_Textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

          

        }
        private bool ValidateExtensions(string extensionsText)
        {
            List<string> extensions = extensionsText.Split(',').ToList();

            foreach (var extension in extensions)
            {
                if (string.IsNullOrWhiteSpace(extension))
                {
                    continue;
                }
                if (!extension.Trim().StartsWith(".") || !extension.Trim().Substring(1).All(char.IsLetterOrDigit))
                {
                    MessageBox.Show(
                        string.Format(_translations.GetStringFromJson("Error_Invalid_Extension_Message"), extension),
                        _translations.GetStringFromJson("Error_Title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }
    }
}
