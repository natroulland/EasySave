using System.Diagnostics;
using EasySave.Logger;
using EasySave.Core.Model;
using EasySave.Core.Model.Entities;

namespace EasySave.ViewModel;

public class JobExecutorController
{
    private readonly List<Observer> observers = new List<Observer> { new LogWriter(), new JobstateWriter() };
    private readonly JobExecutorModel _jobExecutorModel;
    private readonly EncryptionModel _encryptionModel;
    private Dictionary<string, ManualResetEventSlim> pauses = new Dictionary<string, ManualResetEventSlim>();
    private Dictionary<string, ManualResetEventSlim> stopped = new Dictionary<string, ManualResetEventSlim>();
    private List<Job> registered;

    public JobExecutorController()
    {
            _jobExecutorModel = new JobExecutorModel();
            _encryptionModel = new EncryptionModel();
    }

    /// <summary>
    /// Execute a list of jobs
    /// </summary>
    /// <param name="jobs"></param>
    
    public async Task ExecuteJob(List<Job> jobs)
    {
        List<List<Job>> sortedJobs = PrioritizeJobs(jobs);
        List<Job> prio = sortedJobs[0];
        List<Job> notprio = sortedJobs[1];
        await ExecuteSortedJob(prio);
        
        await ExecuteSortedJob(notprio);

    }


    public async Task ExecuteSortedJob(List<Job> jobs)
    {
        // List to store tasks for concurrent execution
        List<Task> tasks = new List<Task>();

        foreach (Job job in jobs)
        // Store the list of jobs in a registered collection
        {
            registered = jobs;
            // Prevent launching the same job if it's already running
            if (pauses.ContainsKey(job.name))
            {
                Debug.WriteLine($"Job {job.name} is already running, skipping duplicate launch.");
                continue;
            }
            if (stopped.ContainsKey(job.name))
            {
                Debug.WriteLine($"Job {job.name} is already running, skipping duplicate launch.");
                continue;
            }

            // Create control mechanisms for pausing and stopping jobs
            ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);
            ManualResetEventSlim stopEvent = new ManualResetEventSlim(true);

            // Store pause and stop events for job management
            pauses[job.name] = pauseEvent;
            stopped[job.name] = stopEvent;

            // Start a new task for the job execution
            tasks.Add(Task.Run(() =>
            {
                Debug.WriteLine($"Starting job {job.name}, pauses count: {pauses.Count}, stopped count: {stopped.Count}");

                // Initialize a stopwatch to measure execution time
                Stopwatch chrono = new Stopwatch();
                chrono.Start();

                // Count the total number of files in the source directory
                int totalFiles = Directory.GetFiles(job.sourcePath, "*", SearchOption.AllDirectories).Length;

                // Create an initial job state dictionary to notify observers
                Dictionary<string, string> datastart = new Dictionary<string, string>
                {
                    { "subject", "Jobstate" },
                    { "Name", job.name },
                    { "FileSource", job.sourcePath },
                    { "FileTarget", job.targetPath },
                    { "State", "In Progress" },
                    { "TotalFileToCopy", totalFiles.ToString() },
                    { "FileTransferTime", "0" },
                    { "NbFileToDo", Directory.GetFiles(job.sourcePath).Length.ToString() },
                    { "Progression", "0" },
                    { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                // Notify observers about the job start state
                UpdateObservers(observers, datastart);

                try
                {
                    // Execute the job (save the files while respecting pause/stop signals)
                    _jobExecutorModel.Save(job, pauseEvent, stopEvent);
                    Debug.WriteLine("save ended");

                    // Stop the stopwatch after the save process is complete
                    chrono.Stop();

                    // Calculate the total size of copied files
                    long totalSize = new DirectoryInfo(job.targetPath)
                        .EnumerateFiles("*", SearchOption.AllDirectories)
                        .Sum(file => file.Length);

                    // Create a dictionary with log data after job completion
                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "subject", "log" },
                        { "Name", job.name },
                        { "FileSource", job.sourcePath },
                        { "FileTarget", job.targetPath },
                        { "FileSize", totalSize.ToString() },
                        { "FileTransferTime", chrono.Elapsed.TotalSeconds.ToString() },
                        { "CryptTime", (chrono.Elapsed.TotalSeconds / totalSize).ToString() },
                        { "Time", DateTime.Now.ToString("HH:mm:ss") }
                    };

                    // Create a final job state dictionary to notify observers
                    Dictionary<string, string> dataEnd = new Dictionary<string, string>
                    {
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

                    
                    Debug.WriteLine("Statut : OK");

                    // Small delay before finalizing
                    Thread.Sleep(1000);

                    // If the stop event is set, encrypt the job's target directory
                    if (stopEvent.IsSet)
                    {
                        Thread encrypting = new Thread(()=> EncryptFilesInDirectory(job.targetPath, job.name));
                        
                        encrypting.Start();
                        Debug.WriteLine("Encrypting");
                        Dictionary<string, string> dataEnc = new Dictionary<string, string>
                            {
                                { "subject", "Jobstate" },
                                { "Name", job.name },
                                { "FileSource", job.sourcePath },
                                { "FileTarget", job.targetPath },
                                { "State", "Encryption" },
                                { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                            };
                        UpdateObservers(observers, dataEnc);
                        encrypting.Join();
                        // Notify observers about job completion
                        UpdateObservers(observers, data);
                        UpdateObservers(observers, dataEnd);
                    }
                    else
                    {
                        // Otherwise, mark the stop event as completed
                        stopEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    // Catch and log any exceptions that occur during execution
                    Debug.WriteLine(ex);
                }
                finally
                {
                    // Cleanup: dispose of pause and stop events to free memory
                    Debug.WriteLine("finally");
                    pauses[job.name].Dispose();
                    stopped[job.name].Dispose();

                    // Remove the job from tracking collections
                    pauses.Remove(job.name);
                    stopped.Remove(job.name);
                    registered.Remove(job);

                    Debug.WriteLine($"Job {job.name} finished and removed from pauses.");

                    // Check if other jobs are still running
                    CheckJobs();
                }
            }));
        }

        // Wait for all tasks to complete before returning
        await Task.WhenAll(tasks);
    }


    public void Pause(string name)// Remote pause, use dictionnary to find the right pause event and deep in the models/save 
    {
        if (pauses.ContainsKey(name) && pauses[name].IsSet)
        {
            pauses[name].Reset();
            Dictionary<string, string> dataPause = new Dictionary<string, string>
            {
                { "subject", "Jobstate" },
                { "Name", name },
                { "State", "Pause" },
                { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            Thread.Sleep(200);
            UpdateObservers(observers, dataPause);
        }
    }

    public void Resume(string name)// Remote resume
    {
        if (pauses.ContainsKey(name) && !pauses[name].IsSet)
        {
            pauses[name].Set();
        }
    }

    public void ResetJob(string name)// Remote reset
    {
        if (registered.Any(j => j.name == name))
        {
            pauses[name] = new ManualResetEventSlim(true);
            Debug.WriteLine($"Job {name} has been reset and can be restarted.");
        }
    }

    public void StopJob(string name)// Remote stop
    {
        if (stopped.ContainsKey(name) && stopped[name].IsSet)
        {
            stopped[name].Reset();
            Dictionary<string, string> dataPause = new Dictionary<string, string>
            {
                { "subject", "Jobstate" },
                { "Name", name },
                { "State", "Killed" },
                { "Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            Thread.Sleep(200);
            UpdateObservers(observers, dataPause);
        }
    }

    static void UpdateObservers(List<Observer> observers, Dictionary<string, string> logData)// Update the observers with logs and jobstates
    {
        foreach (Observer observer in observers)
        {
            observer.Update(logData);
        }
    }

    private void EncryptFilesInDirectory(string directoryPath, string jobName)// Encrypt files (skipped if killed)
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

    public void CheckJobs()//Job status display on debug
    {
        Debug.WriteLine("--- Job Status ---");

        foreach (var job in registered)
        {
            Debug.WriteLine($"Registered job: {job.name}");
        }

        foreach (var key in pauses.Keys)
        {
            Debug.WriteLine($"Paused job: {key}");
        }

        Debug.WriteLine("------------------");
    }
    private List<List<Job>> PrioritizeJobs(List<Job> jobs)
    {
        List<List<Job>> sortedJobs = new List<List<Job>>();
        sortedJobs = _jobExecutorModel.CalculatePrioScore(jobs);

        return sortedJobs;
    }
}
