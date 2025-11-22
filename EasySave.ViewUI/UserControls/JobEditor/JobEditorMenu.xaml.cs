using EasySave.Core.Model;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using EasySave.ViewModel;
using EasySave.Core.Model.Enums;
using EasySave.ViewUI.Languages;
using EasySave.Core.Model.Entities;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasySave.ViewUI.UserControls.JobEditor
{
    /// <summary>
    /// Logique d'interaction pour JobEditorMenu.xaml
    /// </summary>
    public partial class JobEditorMenu : System.Windows.Controls.UserControl
    {
        public ObservableCollection<Job> availableJobs { get; set; }
        private Action action;
        private JobManagerViewModel jobManagerController;
        private Job selectedJob;
        private Language _translations;

        public JobEditorMenu(Language translations)
        {
            InitializeComponent();

            jobManagerController = new();

            availableJobs = GetJobsFromJson();

            DataContext = this;

            _translations = translations;
            TranslateComponents();
        }

        public ObservableCollection<Job> GetJobsFromJson()
        {
            if (!File.Exists("./config.json"))
                return new ObservableCollection<Job>();

            string json = File.ReadAllText("./config.json");
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("JSON is empty");
                return new ObservableCollection<Job>();
            }

            var jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (jsonData != null && jsonData.ContainsKey("Jobs"))
            {
                return JsonSerializer.Deserialize<ObservableCollection<Job>>(jsonData["Jobs"].GetRawText()) ?? new ObservableCollection<Job>();
            }

            return new ObservableCollection<Job>();
        }

        private void Create_Jobs_Button_Click(object sender, RoutedEventArgs e)
        {
            Jobform_grid.Visibility = Visibility.Visible;
            action = Action.Create;
        }

        private void Update_Jobs_Button_Click(object sender, RoutedEventArgs e)
        {
            Jobform_grid.Visibility = Visibility.Visible;
            JobName_Textbox.Text = selectedJob.name;
            SourcePath_Textbox.Text = selectedJob.sourcePath;
            TargetPath_Textbox.Text = selectedJob.targetPath;
            Fullsave_Radio.IsChecked = selectedJob.saveType == SaveType.Full;
            Differentialsave_radio.IsChecked = selectedJob.saveType == SaveType.Differential;

            action = Action.Update;
        }

        private void Sourcepath_button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SourcePath_Textbox.Text = dialog.SelectedPath;
                }
            }
        }

        private void Targetpath_button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    TargetPath_Textbox.Text = dialog.SelectedPath;
                }
            }
        }

        private void Delete_Jobs_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(
                    _translations.GetStringFromJson("Confirmation_Message"),
                    _translations.GetStringFromJson("Confirmation_Title"),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                jobManagerController.DeleteJob(selectedJob, "./config.json");
                availableJobs.Remove(selectedJob);
            }
        }

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Job_listbox.SelectedItem != null)
            {
                selectedJob = e.AddedItems[0] as Job;
                Update_Jobs_Button.IsEnabled = true;
                Delete_Jobs_Button.IsEnabled = true;
            }
            else
            {
                selectedJob = null;
                Update_Jobs_Button.IsEnabled = false;
                Delete_Jobs_Button.IsEnabled = false;
            }
        }

        private void OkJob_Button_Click(object sender, object e)
        {
            if (string.IsNullOrEmpty(JobName_Textbox.Text) || string.IsNullOrEmpty(SourcePath_Textbox.Text) || string.IsNullOrEmpty(TargetPath_Textbox.Text) || (Fullsave_Radio.IsChecked == false && Differentialsave_radio.IsChecked == false))
            {
                System.Windows.MessageBox.Show(
                    _translations.GetStringFromJson("Error_Empty_Fields_Message"),
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK);
                return;
            }
            if (action == Action.Create)
            {
                Job jobToCreate = new Job()
                {
                    name = JobName_Textbox.Text,
                    sourcePath = SourcePath_Textbox.Text,
                    targetPath = TargetPath_Textbox.Text,
                    saveType = Fullsave_Radio.IsChecked == true ? SaveType.Full : SaveType.Differential
                };

                if (availableJobs.Where(j => j.name == jobToCreate.name).Any())
                {
                    System.Windows.MessageBox.Show(
                    $"{_translations.GetStringFromJson("Error_Job_Exists_Message")} {jobToCreate.name}",
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK);
                    return;
                }

                if (jobToCreate.targetPath == jobToCreate.sourcePath)
                {
                    System.Windows.MessageBox.Show(
                    _translations.GetStringFromJson("Error_Same_Source_Target_Message"),
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK);
                    return;
                }

                MessageBoxResult result = System.Windows.MessageBox.Show(
                    string.Format(_translations.GetStringFromJson("Confirmation_Create_Message"), jobToCreate.name, jobToCreate.sourcePath, jobToCreate.targetPath, jobToCreate.saveType),
                    _translations.GetStringFromJson("Confirmation_Title"),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        jobManagerController.ValidateAndSaveJob(jobToCreate, "./config.json");
                        availableJobs.Add(jobToCreate);
                        JobName_Textbox.Text = "";
                        SourcePath_Textbox.Text = "";
                        TargetPath_Textbox.Text = "";
                        Fullsave_Radio.IsChecked = false;
                        Differentialsave_radio.IsChecked = false;

                        Jobform_grid.Visibility = Visibility.Hidden;
                    }
                    catch (ArgumentException ex)
                    {
                        System.Windows.MessageBox.Show(
                        string.Format(_translations.GetStringFromJson("Error_Source_Not_Exists_Message"), jobToCreate.sourcePath),
                        _translations.GetStringFromJson("Error_Title"),
                        MessageBoxButton.OK);
                        return;
                    }
                }
            }
            else
            {
                Job initialJob = selectedJob;

                Job modifiedJob = new Job()
                {
                    name = JobName_Textbox.Text,
                    sourcePath = SourcePath_Textbox.Text,
                    targetPath = TargetPath_Textbox.Text,
                    saveType = Fullsave_Radio.IsChecked == true ? SaveType.Full : SaveType.Differential
                };

                if (modifiedJob.targetPath == modifiedJob.sourcePath)
                {
                    System.Windows.MessageBox.Show(
                    _translations.GetStringFromJson("Error_Same_Source_Target_Message"),
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK);
                    return;
                }

                MessageBoxResult result = System.Windows.MessageBox.Show(
                    string.Format(_translations.GetStringFromJson("Confirmation_Update_Message"), initialJob.name, modifiedJob.name, initialJob.sourcePath, modifiedJob.sourcePath, initialJob.targetPath, modifiedJob.targetPath, initialJob.saveType, modifiedJob.saveType),
                    _translations.GetStringFromJson("Confirmation_Title"),
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        jobManagerController.UpdateJob(modifiedJob, "./config.json", initialJob.name);
                        availableJobs.Remove(initialJob);
                        availableJobs.Add(modifiedJob);
                        JobName_Textbox.Text = "";
                        SourcePath_Textbox.Text = "";
                        TargetPath_Textbox.Text = "";
                        Fullsave_Radio.IsChecked = false;
                        Differentialsave_radio.IsChecked = false;

                        Jobform_grid.Visibility = Visibility.Hidden;
                    }
                    catch (ArgumentException ex)
                    {
                        System.Windows.MessageBox.Show(
                        string.Format(_translations.GetStringFromJson("Error_Source_Not_Exists_Message"), modifiedJob.sourcePath),
                        _translations.GetStringFromJson("Error_Title"),
                        MessageBoxButton.OK);
                        return;
                    }
                }
            }
        }

        public void TranslateComponents()
        {
            Create_Jobs_Button.Content = _translations.GetStringFromJson("Create_Jobs_Button");
            Update_Jobs_Button.Content = _translations.GetStringFromJson("Update_Jobs_Button");
            Delete_Jobs_Button.Content = _translations.GetStringFromJson("Delete_Jobs_Button");
            Fullsave_Radio.Content = _translations.GetStringFromJson("Fullsave_Radio");
            Differentialsave_radio.Content = _translations.GetStringFromJson("Differentialsave_radio");
            Sourcepath_button.Content = _translations.GetStringFromJson("Sourcepath_button");
            Targetpath_button.Content = _translations.GetStringFromJson("Targetpath_button");
            OkJob_Button.Content = _translations.GetStringFromJson("OkJob_Button");
            JobName_Text.Text = _translations.GetStringFromJson("JobName_TextBlock");
            SourcePath_Text.Text = _translations.GetStringFromJson("SourcePath_TextBlock");
            TargetPath_Text.Text = _translations.GetStringFromJson("TargetPath_TextBlock");
            SaveType_Text.Text = _translations.GetStringFromJson("SaveType_TextBlock");
        }

        private void JobName_Textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            Regex regex = new Regex("[^a-zA-Z0-9-_]");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true; 
                errorTextBlock.Text = _translations.GetStringFromJson("JobField_Error");
                errorTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                errorTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void JobName_Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
