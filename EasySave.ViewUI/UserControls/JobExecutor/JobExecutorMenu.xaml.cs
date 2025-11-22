using System.Windows;
using System.Windows.Controls;
using EasySave.ViewModel;
using EasySave.Core.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using EasySave.ViewUI.Languages;
using System.Windows.Documents;
using EasySave.Core.Model.Entities;
using EasySave.Core.Model.Enums;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using static EasySave.ViewUI.UserControls.JobExecutor.JobExecutorMenu;

namespace EasySave.ViewUI.UserControls.JobExecutor
{
    /// <summary>
    /// Logique d'interaction pour JobExecutorMenu.xaml
    /// </summary>
    public partial class JobExecutorMenu : UserControl
    {
        private System.Timers.Timer _timer;
        public List<Job> Jobs { get; set; } = new();
        public ObservableCollection<selectJob> availableJobs { get; set; }
        public bool isChecked { get; set; }
        private JobExecutorController _jobExecutorController;
        public ObservableCollection<JobState> jobStateList { get; set; }
        private Language _translations;

        private Socket _client;
        private readonly object _clientLock = new object();
        private readonly ManualResetEventSlim _clientChanged = new ManualResetEventSlim(false);

        public Socket Client
        {
            get
            {
                lock (_clientLock)
                {
                    return _client;
                }
            }
            set
            {
                lock (_clientLock)
                {
                    _client = value;
                    _clientChanged.Set(); // Notifier le thread du changement
                }
            }
        }
        private List<JobState> lastJobstateSend = new();
        private JobState actualJobState = new();

        public JobExecutorMenu(Language translations)
        {
            jobStateList = new ObservableCollection<JobState>();
            InitializeComponent();
            GetJobsFromJson();
            GetJobState();

            _jobExecutorController = new();

            DataContext = this;

            _timer = new System.Timers.Timer(100);
            _timer.Elapsed += (s, e) =>
            {
                if (Application.Current == null)
                {
                    _timer.Stop();
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    GetJobState();
                });
            };
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();

            _translations = translations;
            TranslateComponents();
        }

        /// <summary>
        /// Retrieve jobs from the JSON file
        /// </summary>
        public void GetJobsFromJson()
        {
            if (!File.Exists("./config.json"))
                return;

            string json = File.ReadAllText("./config.json");

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("JSON is empty");
                return;
            }

            var jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (jsonData != null && jsonData.ContainsKey("Jobs"))
            {
                Jobs = JsonSerializer.Deserialize<List<Job>>(jsonData["Jobs"].GetRawText()) ?? new List<Job>();
            }

            availableJobs = new ObservableCollection<selectJob>(
                Jobs.Select((job) => new selectJob
                {
                    isChecked = false,
                    Job = job
                })
            );
        }

        /// <summary>
        /// IndexedJob class bind data in view
        /// </summary>
        public class selectJob
        {
            public bool isChecked { get; set; }
            public Job Job { get; set; }
        }

        /// <summary>
        /// Read real time jobstate file
        /// </summary>
        public void GetJobState()
        {
            if (!File.Exists("./Jobstate/jobstate.json"))
                return;

            using (var file_stream = File.Open("./Jobstate/jobstate.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var stream_reader = new StreamReader(file_stream))
            {
                string jsonJobState = stream_reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(jsonJobState))
                    return;

                List<JobStateEntity> jobStates = JsonSerializer.Deserialize<List<JobStateEntity>>(jsonJobState) ?? new List<JobStateEntity>();

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    jobStateList.Clear();

                    foreach (var js in jobStates)
                    {
                        string progress = js.Progression; // Exemple de chaîne "x/y"

                        // Diviser la chaîne en deux parties
                        string[] parts = progress.Split('/');
                        double percentage = 0;

                        if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                        {
                            // Calculer le pourcentage
                            percentage = (double)x / y * 100;
                        }
                        var jobState = new JobState { JobStateEntity = js, ProgressBar = percentage };
                        jobStateList.Add(jobState);

                        actualJobState = jobState;
                    }
                });
            }
        }

        public async Task SendProgressToClient()
        {
            while (true)
            {
                Socket client;
                lock (_clientLock)
                {
                    client = _client;
                }

                if (client != null)
                {
                    if (!lastJobstateSend.Any(js => js.JobStateEntity.Name == actualJobState.JobStateEntity.Name))
                    {
                        lastJobstateSend.Add(actualJobState);
                    }

                    var existingJobState = lastJobstateSend.FirstOrDefault(js => js.JobStateEntity.Name == actualJobState.JobStateEntity.Name);
                    if (existingJobState != null && existingJobState.ProgressBar != actualJobState.ProgressBar)
                    {
                        existingJobState.ProgressBar = actualJobState.ProgressBar;

                        var jobToSend = new
                        {
                            actualJobState.JobStateEntity.Name,
                            actualJobState.ProgressBar,
                            actualJobState.JobStateEntity.State
                        };

                        var datasToClient = new
                        {
                            title = "progress",
                            datas = JsonSerializer.Serialize(jobToSend)
                        };
                        string json = JsonSerializer.Serialize(datasToClient);
                        byte[] data = Encoding.UTF8.GetBytes(json);

                        // Envoyer le message
                        await client.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

                        Thread.Sleep(100);
                    }
                }
                else
                {
                    // Attendre que le client soit défini
                    _clientChanged.Wait();
                    _clientChanged.Reset();
                }
            }
        }

        public async Task SendJobsToClient(Socket client)
        {
            try
            {
                var datasToClient = new
                {
                    title = "jobs",
                    datas = JsonSerializer.Serialize(Jobs)
                };

                string json = JsonSerializer.Serialize(datasToClient);
                byte[] data = Encoding.UTF8.GetBytes(json);

                // Envoi des données
                await client.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

                Thread.Sleep(5000);
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Error: " + e.Message));
            }
        }


        public async Task ExecuteJobsForClient(Socket client, List<Job> jobs)
        {
            await _jobExecutorController.ExecuteJob(jobs);
        }

        public async Task SetClient(Socket client)
        {
            this.Client = client;

            await Task.Run(() => SendJobsToClient(client));

            Thread thClient = new Thread(() => SendProgressToClient());
            thClient.Start();
        }

        public class JobState
        {
            public JobStateEntity JobStateEntity { get; set; }
            public double ProgressBar { get; set; }
        }

        public class JobStateEntity
        {

            public string Name { get; set; }
            public string FileSource { get; set; }
            public string FileTarget { get; set; }
            public string State { get; set; }
            public string TotalFileToCopy { get; set; }
            public string FileTransferTime { get; set; }
            public string NbFileToDo { get; set; }
            public string Progression { get; set; }
            public string Timestamp { get; set; }
        }

        public void Dispose()
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void executejob_button_Click(object sender, RoutedEventArgs e)
        {
            if (!availableJobs.Any(job => job.isChecked == true))
            {
                MessageBox.Show(
                    _translations.GetStringFromJson("Error_No_Job_Selected_Message"),
                    _translations.GetStringFromJson("Error_Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                _translations.GetStringFromJson("Confirmation_Execute_Message"),
                _translations.GetStringFromJson("Confirmation_Title"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var selectedJobs = availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();
                await _jobExecutorController.ExecuteJob(selectedJobs);
            }
            else
                return;
        }

        public void TranslateComponents()
        {
            executejob_button.Content = _translations.GetStringFromJson("Execute_job_button");
            Available_jobs_TextBlock.Text = _translations.GetStringFromJson("Available_jobs_TextBlock");
            Selected_jobs_TextBlock.Text = _translations.GetStringFromJson("Selected_jobs_TextBlock");

            // DataGrid Headers
            foreach (var column in DataGrid_JobState.Columns)
            {
                if (column is DataGridTextColumn textColumn)
                {
                    switch (textColumn.Header.ToString())
                    {
                        case "Timestamp":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_Timestamp_Header");
                            break;
                        case "Job name":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_Job_name_Header");
                            break;
                        case "State":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_State_Header");
                            break;
                        case "Source":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_Source_Header");
                            break;
                        case "File to copy":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_File_to_copy_Header");
                            break;
                        case "File transfer time":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_File_transfer_time_Header");
                            break;
                        case "File to do":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_File_to_do_Header");
                            break;
                        case "Progression":
                            textColumn.Header = _translations.GetStringFromJson("DataGrid_Progression_Header");
                            break;
                    }
                }
            }
        }

        private void stopjob_button_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobs = availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();

            foreach (var jobName in selectedJobs)
            {
                _jobExecutorController.StopJob(jobName.name);
            }
        }


        private void resume_button_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobs = availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();

            foreach (var jobName in selectedJobs)
            {
                _jobExecutorController.Resume(jobName.name);
            }
        }

        private void pause_button_Click(object sender, RoutedEventArgs e)
        {
            var selectedJobs = availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();

            foreach (var jobName in selectedJobs)
            {
                _jobExecutorController.Pause(jobName.name);
            }
        }

        public void HandlePauseClient(List<Job> selectedJobs)
        {
            foreach (var job in selectedJobs)
            {
                _jobExecutorController.Pause(job.name);
            }
        }
        
        public void HandleResumeClient(List<Job> selectedJobs)
        {
            foreach (var job in selectedJobs)
            {
                _jobExecutorController.Resume(job.name);
            }
        }
        
        public void HandleStopClient(List<Job> selectedJobs)
        {
            foreach (var job in selectedJobs)
            {
                _jobExecutorController.StopJob(job.name);
            }
        }
    }
}

