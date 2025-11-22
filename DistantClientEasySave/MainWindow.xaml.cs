using DistantClientEasySave.View.Entities;
using EasySave.Core.Model.Entities;
using EasySave.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using static EasySave.ViewUI.UserControls.JobExecutor.JobExecutorMenu;

namespace DistantClientEasySave;

public partial class MainWindow : Window
{
    private Socket clientSocket;
    private ObservableCollection<Job> Jobs = new();
    private ObservableCollection<selectJob> _availableJobs = new();
    public ObservableCollection<JobState> jobStateList { get; set; }

    public ObservableCollection<selectJob> AvailableJobs
    {
        get => _availableJobs;
        set
        {
            _availableJobs = value;
            OnPropertyChanged(nameof(AvailableJobs));
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        AvailableJobs = new();
        clientSocket = ConnectToServer();

        InitializeAsync();

        jobStateList = new();
        DataContext = this; // Assurez-vous que le DataContext est défini
    }

    /// <summary>
    /// Listening server messages
    /// </summary>
    /// <returns></returns>
    private async Task InitializeAsync()
    {
        // Démarrer le thread poxur écouter le serveur
        Thread thClient = new Thread(() => ListenToServer(clientSocket));
        thClient.Start();
    }

    private Socket ConnectToServer()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            socket.Connect("127.0.0.1", 12345);
            Dispatcher.Invoke(() => MessageBox.Show("Connected!"));
            return socket;
        }
        catch (Exception e)
        {
            Dispatcher.Invoke(() => MessageBox.Show("Connection failed: " + e.Message));
            return null;
        }
    }

    private async void ListenToServer(Socket clientSocket)
    {
        try
        {
            while (true)
            {
                using (var memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesReceived;

                    // Lire les données jusqu'à ce qu'il n'y ait plus de données à lire
                    while ((bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None)) > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesReceived);

                        // Si le serveur ferme la connexion, sortir de la boucle
                        if (bytesReceived < buffer.Length)
                        {
                            break;
                        }
                    }

                    if (bytesReceived == 0) break; // Connection closed by server

                    string jsonReceived = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var datasFromServer = JsonSerializer.Deserialize<ServerDatas>(jsonReceived);

                    if (datasFromServer.title == "progress")
                    {
                        JobState jobState = JsonSerializer.Deserialize<JobState>(datasFromServer.datas);
                        Dispatcher.Invoke(() =>
                        {
                            var existingJobState = jobStateList.FirstOrDefault(js => js.Name == jobState.Name);
                            if (existingJobState == null)
                            {
                                jobStateList.Add(jobState);
                            }
                            else
                            {
                                existingJobState.State = jobState.State;
                                existingJobState.ProgressBar = jobState.ProgressBar;
                            }
                        });
                    }
                    else
                    {
                        try
                        {
                            var listJob = JsonSerializer.Deserialize<List<Job>>(datasFromServer.datas);
                            Dispatcher.Invoke(() =>
                            {
                                foreach (var job in listJob)
                                {
                                    selectJob selectJob = new selectJob
                                    {
                                        isChecked = false,
                                        Job = job
                                    };

                                    AvailableJobs.Add(selectJob);
                                }
                            });
                        }
                        catch (JsonException ex)
                        {
                            Dispatcher.Invoke(() => MessageBox.Show($"JSON deserialization error: {ex.Message}"));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => MessageBox.Show("Error in ListenToServer: " + ex.Message));
        }
    }



    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void executejob_button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_availableJobs.Any(job => job.isChecked == true))
            {
                MessageBox.Show(
                    "Aucun job n'est selectionné",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Voulez-vous vraiment exécuter ces jobs",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var selectedJobs = _availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();
                jobStateList.Clear();
                ActionInfos actionInfos = new()
                {
                    Action = "executeJobs",
                    Jobs = selectedJobs
                };

                string json = JsonSerializer.Serialize(actionInfos);
                byte[] data = Encoding.UTF8.GetBytes(json);

                // Envoi de la taille des données
                byte[] sizeInfo = BitConverter.GetBytes(data.Length);
                await clientSocket.SendAsync(new ArraySegment<byte>(sizeInfo), SocketFlags.None);

                // Envoi des données
                await clientSocket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
            }
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => MessageBox.Show("Error: " + ex.Message));
        }
    }

    private async void stopjob_button_Click(object sender, RoutedEventArgs e)
    {
        var selectedJobs = _availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();

        ActionInfos actionInfos = new()
        {
            Action = "stopJobs",
            Jobs = selectedJobs
        };

        string json = JsonSerializer.Serialize(actionInfos);
        byte[] data = Encoding.UTF8.GetBytes(json);

        // Envoi de la taille des données
        byte[] sizeInfo = BitConverter.GetBytes(data.Length);
        await clientSocket.SendAsync(new ArraySegment<byte>(sizeInfo), SocketFlags.None);

        // Envoi des données
        await clientSocket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
    }


    private async void resume_button_Click(object sender, RoutedEventArgs e)
    {
        var selectedJobs = _availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();

        ActionInfos actionInfos = new()
        {
            Action = "resumeJobs",
            Jobs = selectedJobs
        };

        string json = JsonSerializer.Serialize(actionInfos);
        byte[] data = Encoding.UTF8.GetBytes(json);

        // Envoi de la taille des données
        byte[] sizeInfo = BitConverter.GetBytes(data.Length);
        await clientSocket.SendAsync(new ArraySegment<byte>(sizeInfo), SocketFlags.None);

        // Envoi des données
        await clientSocket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
    }

    private async void pause_button_Click(object sender, RoutedEventArgs e)
    {
        var selectedJobs = _availableJobs.Where(job => job.isChecked).Select(job => job.Job).ToList();

        ActionInfos actionInfos = new()
        {
            Action = "pauseJobs",
            Jobs = selectedJobs
        };

        string json = JsonSerializer.Serialize(actionInfos);
        byte[] data = Encoding.UTF8.GetBytes(json);

        // Envoi de la taille des données
        byte[] sizeInfo = BitConverter.GetBytes(data.Length);
        await clientSocket.SendAsync(new ArraySegment<byte>(sizeInfo), SocketFlags.None);

        // Envoi des données
        await clientSocket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
    }
}

public class JobState : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _state;
    private double _progressBar;

    public string Name { get; set; }

    public string State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }

    public double ProgressBar
    {
        get => _progressBar;
        set
        {
            if (_progressBar != value)
            {
                _progressBar = value;
                OnPropertyChanged(nameof(ProgressBar));
            }
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}