using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasySave.Logger;
using EasySave.Model;
using EasySave.Model.Enums;
using EasySave.Model.SaveStrategies;

namespace EasySave.ViewModel
{
    public class JobExecutorController
    {
        List<Observer> observers = new List<Observer> { new LogWriter(), new JobstateWriter() };
        private readonly JobExecutorModel _jobExecutorModel;
        private readonly EncryptionModel _encryptionModel;

        public JobExecutorController()
        {
            _jobExecutorModel = new JobExecutorModel();
            _encryptionModel = new EncryptionModel();
        }

        /// <summary>
        /// Execute a list of jobs
        /// </summary>
        /// <param name="jobs"></param>

        public async void ExecuteJob(List<Job> jobs)
        {
            await Task.Run(() =>
            {
                foreach (Job job in jobs)
                {
                    Stopwatch chrono = new Stopwatch();
                    chrono.Start();
                    int totalFiles = Directory.GetFiles(job.sourcePath, "*", SearchOption.AllDirectories).Length;
                    Dictionary<string, string> datastart = new Dictionary<string, string> {
                { "subject", "Jobstate" },
                { "Name", job.name },
                { "FileSource", job.sourcePath },
                { "FileTarget", job.targetPath },
                { "State", "In Progress" },
                { "TotalFileToCopy", totalFiles.ToString() },
                { "FileTransferTime", "0" },
                { "NbFileToDo", Directory.GetFiles(job.sourcePath).Length.ToString()},
                { "Progression", "0" },
                { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

                    UpdateObservers(observers, datastart);
                    try
                    {
                        _jobExecutorModel.Save(job);
                        chrono.Stop();
                        long taille = new DirectoryInfo(job.targetPath)
                            .EnumerateFiles("*", SearchOption.AllDirectories)
                            .Sum(file => file.Length);
                        Dictionary<string, string> data = new Dictionary<string, string>
                        {
                            { "subject", "log" },
                            { "Name", job.name },
                            { "FileSource", job.sourcePath },
                            { "FileTarget", job.targetPath},
                            { "FileSize", taille.ToString() },
                            { "FileTransferTime", chrono.Elapsed.TotalSeconds.ToString()},
                            { "CryptTime",(chrono.Elapsed.TotalSeconds/taille).ToString()},
                            { "Time", DateTime.Now.ToString("HH:mm:ss") }
                        };
                        Dictionary<string, string> dataEnd = new Dictionary<string, string> {
                        { "subject", "Jobstate" },
                        { "Name", job.name },
                        { "FileSource", job.sourcePath },
                        { "FileTarget", job.targetPath },
                        { "State", "End" },
                        { "TotalFileToCopy", "0" },
                        { "FileTransferTime", "0" },
                        { "NbFileToDo", "0" },
                        { "Progression", "0" },
                        { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                    };
                        UpdateObservers(observers, data);
                        UpdateObservers(observers, dataEnd);
                        Console.WriteLine("Statut : OK");
                        Thread.Sleep(1000);

                        EncryptFilesInDirectory(job.targetPath, job.name);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
            static void UpdateObservers(List<Observer> observers, Dictionary<string, string> logData)
            {
                foreach (Observer observer in observers)
                {
                    observer.Update(logData);
                }
            }
        }

        private void EncryptFilesInDirectory(string directoryPath, string jobName)
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                if (_encryptionModel.ShouldEncryptFile(filePath))
                {
                    Console.WriteLine($"Encrypting file: {filePath}, Job: {jobName}");

                    string hash = _encryptionModel.ComputeHash(jobName);
                    _encryptionModel.SaveHashAndFileName(filePath, jobName, Path.GetFileName(filePath), hash);
                    _encryptionModel.ExecuteEncryptionProcess(filePath, hash);
                }
            }
        }



    }
}
