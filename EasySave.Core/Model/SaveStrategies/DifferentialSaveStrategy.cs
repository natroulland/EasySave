using EasySave.Core.Model.Entities;
using EasySave.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EasySave.Core.Model.SaveStrategies;

public class DifferentialSaveStrategy : ISaveStrategy
{
    /// <summary>
    /// Execute a diffential save
    /// </summary>
    /// <param name="job"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    List<Observer> observers = new List<Observer> { new JobstateWriter() };
    public void Save(Job job, int lasttimeupdate, Job ParentJob, Stopwatch parentchrono, ref int number, ManualResetEventSlim pauseEvent, ManualResetEventSlim stopEvent)

    {
        pauseEvent.Wait();
        if (!Directory.Exists(job.sourcePath)) // if source path does not exist
            throw new DirectoryNotFoundException($"Source folder '{job.sourcePath}' not found.");

        Directory.CreateDirectory(job.targetPath);
        int totalFiles = Directory.GetFiles(ParentJob.sourcePath, "*", SearchOption.AllDirectories).Length;
        foreach (string file in Directory.GetFiles(job.sourcePath)) // f or each file in directory, copying into target
        {
            string destinationFile = Path.Combine(job.targetPath, Path.GetFileName(file));
            if (File.Exists(destinationFile)) // check if the file exists in target path 
            {
                pauseEvent.Wait();
                if (!stopEvent.IsSet ) { return; }
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
                


                DateTime dateFileSource = File.GetLastWriteTime(file);
                DateTime dateFileDest = File.GetLastWriteTime(destinationFile);

                if (dateFileSource > dateFileDest) //check if the last update of the source file is more recent in than the target one
                {
                    // if it does, copying file, else pass
                    File.Copy(file, destinationFile, true);
                }
            }
            else
            {
                File.Copy(file, destinationFile, true);
            }
            number++;
            Dictionary<string, string> data = new Dictionary<string, string>
        {
            { "subject", "Jobstate" },
            { "Name", ParentJob.name },
            {"State", "In Progress" },
            { "FileTransferTime", parentchrono.Elapsed.ToString() },
            { "NbFileToDo", (totalFiles - number).ToString() },
            { "Progression", number.ToString()+ "/"+totalFiles.ToString() },
            { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };
            lasttimeupdate = UpdateObservers(observers, data, lasttimeupdate);
        }

        foreach (string directory in Directory.GetDirectories(job.sourcePath)) // For each directory in directory, doing the same entire logic
        {
            pauseEvent.Wait();
            if (!stopEvent.IsSet ) { return; }
            Job newJob = new()
            {
                name = job.name,
                sourcePath = Path.Combine(job.sourcePath, Path.GetFileName(directory)),
                targetPath = Path.Combine(job.targetPath, Path.GetFileName(directory)),
            };
            Save(newJob, lasttimeupdate, ParentJob, parentchrono, ref number, pauseEvent, stopEvent);
        }
        static int UpdateObservers(List<Observer> observers, Dictionary<string, string> logData, int lasttimeupdate, bool forceUpdate = false)
        {
            int currentSecond = DateTime.Now.Millisecond/100;

            // If it's the same second as before and not a forced update, skip logging
            if (!forceUpdate && currentSecond == lasttimeupdate) return lasttimeupdate;

            lasttimeupdate = currentSecond;

            foreach (Observer observer in observers)
            {
                observer.Update(logData);
            }
            return lasttimeupdate;
        }

    }
    public bool DetectProcess()
    {
        if (string.IsNullOrEmpty(GetBusinessSoftware()))
        {
            return false;
        }
        Process[] pname = Process.GetProcessesByName(GetBusinessSoftware().Replace(".exe", ""));
        if (pname.Length == 0)
            return false;
        else
            return true;
    }

    public string GetBusinessSoftware()
    {
        string filePath = "config.json";

        string json = File.ReadAllText(filePath);
        JObject jsonObj = JObject.Parse(json);

        return jsonObj["BusinessSoftware"].ToString();
    }
}
