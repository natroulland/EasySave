using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using EasySave.Core.Model.Entities;
using EasySave.Logger;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;

public class FullSaveStrategy : ISaveStrategy
{
    List<Observer> observers = new List<Observer> { new JobstateWriter(), new LogWriter() };

    public void Save(Job job, int lasttimeupdate, Job ParentJob, Stopwatch parentchrono, ref int number, ManualResetEventSlim pauseEvent, ManualResetEventSlim stopEvent)
    {
        pauseEvent.Wait(); if (!stopEvent.IsSet) { return; }

        if (!Directory.Exists(job.sourcePath))
            throw new DirectoryNotFoundException($"Source folder '{job.sourcePath}' not found.");

        if (Directory.Exists(job.targetPath))
        {
            DirectoryInfo target = new DirectoryInfo(job.targetPath);
            target.Delete(true);
        }

        if (!Directory.Exists(job.targetPath))
            Directory.CreateDirectory(job.targetPath);

        int totalFiles = Directory.GetFiles(ParentJob.sourcePath, "*", SearchOption.AllDirectories).Length;

        foreach (string file in SortFiles(job.sourcePath))
        {
            pauseEvent.Wait(); if (!stopEvent.IsSet) { return; }

            if (DetectProcess())
            {
                Dictionary<string, string> dataStopped = new Dictionary<string, string>
                {
                    { "subject", "log" },
                    { "Name", job.name },
                    { "FileSource", job.sourcePath },
                    { "FileTarget", job.targetPath },
                    { "FileSize", "Error Occurred" },
                    { "FileTransferTime", "Business Software is running, save is self-paused by caution" },
                    { "Time", DateTime.Now.ToString("HH:mm:ss") }
                };
                lasttimeupdate = UpdateObservers(observers, dataStopped, lasttimeupdate, true);

                Dictionary<string, string> dataFailed = new Dictionary<string, string>
                {
                    { "subject", "Jobstate" },
                    { "Name", ParentJob.name },
                    { "State", "Buisness Software Detected : Pause" },
                    { "FileTransferTime", parentchrono.Elapsed.ToString() },
                    { "NbFileToDo", "0" },
                    { "Progression", "0" },
                    { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };
                lasttimeupdate = UpdateObservers(observers, dataFailed, lasttimeupdate, true);
            }

            while (DetectProcess())
            {
                pauseEvent.Wait();
                // Log the stop due to business software detection


                Debug.WriteLine("Business Software is running, save is self-stopped by caution");
                if (pauseEvent.IsSet) pauseEvent.Reset();

            }

            Debug.WriteLine(file);

            number++;
            string destFile = Path.Combine(job.targetPath, Path.GetFileName(file));
            File.Copy(file, destFile, true);

            // Update job state
            Dictionary<string, string> data = new Dictionary<string, string>
        {
            { "subject", "Jobstate" },
            { "Name", ParentJob.name },
            { "FileTransferTime", parentchrono.Elapsed.ToString() },
            { "State", "In Progress" },
            { "NbFileToDo", (totalFiles - number).ToString() },
            { "Progression", number.ToString() + "/" + totalFiles.ToString() },
            { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
            lasttimeupdate = UpdateObservers(observers, data, lasttimeupdate);
        }

        foreach (string directory in Directory.GetDirectories(job.sourcePath)
                .OrderByDescending(dir => Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length)))
        {
            Debug.WriteLine(directory);
            pauseEvent.Wait(); if (!stopEvent.IsSet) { return; }

            Job newJob = new()
            {
                name = job.name,
                sourcePath = Path.Combine(job.sourcePath, Path.GetFileName(directory)),
                targetPath = Path.Combine(job.targetPath, Path.GetFileName(directory)),
            };

            Save(newJob, lasttimeupdate, ParentJob, parentchrono, ref number, pauseEvent, stopEvent);
        }
    }

    /// <summary>
    /// Associate a file with a "Deepness score" based on the number of separator
    /// </summary>
    public class FolderPriorityWrapper()
    {
        public int prioScore;
        public string folder;
    }
    private int UpdateObservers(List<Observer> observers, Dictionary<string, string> logData, int lasttimeupdate, bool forceUpdate = false)
    {
        int currentSecond = DateTime.Now.Microsecond / 100;

        // If it's the same second as before and not a forced update, skip logging
        if (!forceUpdate && currentSecond == lasttimeupdate) return lasttimeupdate;

        lasttimeupdate = currentSecond;

        foreach (Observer observer in observers)
        {
            observer.Update(logData);
        }
        return lasttimeupdate;
    }
    // Function to update observers with job state


    // Function to detect if a business software process is running
    public bool DetectProcess()
    {
        if (string.IsNullOrEmpty(GetBusinessSoftware()))
        {
            return false;
        }
        Process[] pname = Process.GetProcessesByName(GetBusinessSoftware().Replace(".exe", ""));
        return pname.Length > 0;
    }

    // Function to retrieve the business software name from config
    public string GetBusinessSoftware()
    {
        string filePath = "config.json";

        string json = File.ReadAllText(filePath);
        JObject jsonObj = JObject.Parse(json);

        return jsonObj["BusinessSoftware"].ToString();
    }


    /// <summary>
    /// Sort files : Prioritized extensions > Folders > Size
    /// </summary>
    /// <param name="rootDir">Source path to copy</param>
    /// <returns>Sorted files</returns>
    public static List<string> SortFiles(string dir)
    {
        List<string> priorityExtensions = GetPrioritizedExtensions();

        var sortedFiles = Directory
                        .EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(file => new FileInfo(file).Length)
                        .ToList();

        sortedFiles = sortedFiles
                    .OrderBy(file => priorityExtensions.Contains(Path.GetExtension(file).ToLower()) ? 0 : 1)
                    .ToList();

        return sortedFiles;
    }

    public static List<string> GetPrioritizedExtensions()
    {
        List<string>? _prioritizedExtensions;
        string filePath = "config.json";

        if (!File.Exists(filePath))
        {
            _prioritizedExtensions = new List<string>();
            return _prioritizedExtensions;
        }

        string json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.WriteLine("JSON is empty");
            _prioritizedExtensions = new List<string>();
            return _prioritizedExtensions;
        }

        JObject jsonObj = JObject.Parse(json);
        return _prioritizedExtensions = jsonObj["ExtensionsPriority"]?.ToObject<List<string>>();
    }
}
