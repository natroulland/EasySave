using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using EasySave.ViewUI.Languages;
using EasySave.ViewUI.UserControls;
using EasySave.ViewUI.UserControls.DecryptionWindow;
using EasySave.ViewUI.UserControls.JobEditor;
using EasySave.ViewUI.UserControls.JobExecutor;
using EasySave.ViewUI.UserControls.Settings;
using Newtonsoft.Json.Linq;
using EasySave.Core.Model.Entities;
using EasySave.Core.Model.Enums;
using System.Text.Json;
using EasySave.ViewModel;

namespace EasySave.ViewUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainFrame : Window
{

    private JobExecutorMenu _jobExecutorMenu;
    private JobEditorMenu _jobEditorMenu;
    private SettingsMenu _settingsMenu;
    private string _language;
    private Language _translations;
    private DecryptionWindowMenu _decryptionmenu;
    private Socket serverSocket;

    public MainFrame()
    {
        InitializeComponent();
        try
        {
            HomePage homePage = new HomePage();
            ContentFrame.Content = homePage;
            
        }
        catch (Exception ex)
        {
            throw new Exception("caca");
        }
        if (!File.Exists("./config.json"))
        {
            using (File.Create("config.json")) { }

            JObject jsonObject = new()
            {
                ["Jobs"] = new JArray(),
                ["Language"] = "English",
                ["LogType"] = "json",
                ["Extensions"] = new JArray(),
                ["ExtensionsPriority"] = new JArray(),
                ["BusinessSoftware"] = "",
                ["MaxFileSizeKo"] = 10000
            };

            File.WriteAllText("./config.json", jsonObject.ToString());
        }
        _language = GetLanguage();
        if (_language == "English")
        {
            _translations = new("../../../Languages/english.json");
        }
        else
        {
            _translations = new("../../../Languages/french.json");
        }

        TranslateComponents();
        // ContentFrame.Content = _jobExecutorMenu;

        double captionHeight = SystemParameters.CaptionHeight;
        double captionWidth = SystemParameters.CaptionWidth;
        this.Height = 475 + 2 * captionHeight;
        this.MinHeight = 475 + 2 * captionHeight;
        this.Width = 800 + captionWidth/2;
        this.MinWidth = 800 + captionWidth/2;


        _jobExecutorMenu = new(_translations);
        _decryptionmenu = new(_translations);

        InitializeServer();
    }

    private void InitializeServer()
    {
        serverSocket = StartServer();
        if (serverSocket != null)
        {
            Thread thServer = new Thread(() => AcceptConnections(serverSocket));
            thServer.IsBackground = true;
            thServer.Start();
        }
    }

    private Socket StartServer()
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 12345);

        try
        {
            socket.Bind(endPoint);
            socket.Listen(10);
            return socket;
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => MessageBox.Show("Error starting server: " + ex.Message));
            return null;
        }
    }

    public async Task ListenToClient(Socket client)
    {
        try
        {
            while (true)
            {
                byte[] sizeInfo = new byte[4];
                int bytesReceived = await client.ReceiveAsync(new ArraySegment<byte>(sizeInfo), SocketFlags.None);
                if (bytesReceived == 0)
                {
                    MessageBox.Show("Connection stopped by server");
                    break;
                }

                int dataLength = BitConverter.ToInt32(sizeInfo, 0);
                byte[] dataReceived = new byte[dataLength];
                int totalBytesReceived = 0;
                while (totalBytesReceived < dataLength)
                {
                    bytesReceived = await client.ReceiveAsync(new ArraySegment<byte>(dataReceived, totalBytesReceived, dataLength - totalBytesReceived), SocketFlags.None);
                    if (bytesReceived == 0)
                    {
                        break;
                    }
                    totalBytesReceived += bytesReceived;
                }

                if (totalBytesReceived != dataLength)
                {
                    break;
                }

                string jsonReceived = Encoding.UTF8.GetString(dataReceived, 0, totalBytesReceived);

                var actions = JsonSerializer.Deserialize<ActionInfos>(jsonReceived);

                // Appeler une fonction en fonction de l'action
                switch (actions.Action.Trim())
                {
                    case "executeJobs":
                        Task.Run(async () => await _jobExecutorMenu.ExecuteJobsForClient(client, actions.Jobs));
                        break;
                    case "stopJobs":
                        Task.Run(() => _jobExecutorMenu.HandleStopClient(actions.Jobs));
                        break;
                    case "pauseJobs":
                        Task.Run(() => _jobExecutorMenu.HandlePauseClient(actions.Jobs));
                        break;
                    case "resumeJobs":
                        Task.Run(() => _jobExecutorMenu.HandleResumeClient(actions.Jobs));
                        break;
                    default:
                        Console.WriteLine("Unknown action.");
                        break;
                }
            }
        }
        catch (Exception e)
        {
        }
        finally
        {
            Disconnect(client);
        }
    }

    private void Disconnect(Socket socket)
    {
        try
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        catch (Exception e)
        {
            Dispatcher.Invoke(() => MessageBox.Show("Error closing connection: " + e.Message));
        }
    }

    private async void AcceptConnections(Socket server)
    {
        while (true)
        {
            Socket clientSocket = server.Accept();
            await _jobExecutorMenu.SetClient(clientSocket);
            Thread clientThread = new Thread(() => ListenToClient(clientSocket));
            clientThread.IsBackground = true;
            clientThread.Start();
        }
    }



    private void JobExecutorButton_Click(object sender, RoutedEventArgs e)
    {
        TranslateComponents();
        _language = GetLanguage();
        if (_language == "English")
        {
            _translations = new("../../../Languages/english.json");
        }
        else
        {
            _translations = new("../../../Languages/french.json");
        }
        _jobExecutorMenu = new(_translations);
        this.ContentFrame.Content = _jobExecutorMenu;
    }

    private void JobEditorButton_Click(object sender, RoutedEventArgs e)
    {
        TranslateComponents();
        _language = GetLanguage();
        if (_language == "English")
        {
            _translations = new("../../../Languages/english.json");
        }
        else
        {
            _translations = new("../../../Languages/french.json");
        }
        _jobEditorMenu = new(_translations);
        this.ContentFrame.Content = _jobEditorMenu;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        TranslateComponents();
        _language = GetLanguage();
        if (_language == "English")
        {
            _translations = new("../../../Languages/english.json");
        }
        else
        {
            _translations = new("../../../Languages/french.json");
        }
        _settingsMenu = new(_translations);
        this.ContentFrame.Content = _settingsMenu;
    }
    public string GetLanguage()
    {
        string filePath = "config.json";
        string json = File.ReadAllText(filePath);
        JObject jsonObj = JObject.Parse(json);

        return jsonObj["Language"].ToString();
    }

    public void TranslateComponents()
    {
        JobEditorButton.Content = _translations.GetStringFromJson("JobEditorButton");
        JobExecutorButton.Content = _translations.GetStringFromJson("JobExecutorButton");
        SettingsButton.Content = _translations.GetStringFromJson("SettingsButton");
        FileDecryptionButton.Content = _translations.GetStringFromJson("FileDecryptionButton");
    }
    private void DecryptButton_Click(object sender, RoutedEventArgs e)
    {
        TranslateComponents();
        _language = GetLanguage();
        if (_language == "English")
        {
            _translations = new("../../../Languages/english.json");
        }
        else
        {
            _translations = new("../../../Languages/french.json");
        }
        _decryptionmenu = new(_translations);
        this.ContentFrame.Content = _decryptionmenu;
    }


}