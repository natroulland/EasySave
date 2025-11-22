using EasySave.Core.Model.Entities;
using EasySave.Core.Model.Enums;
using EasySave.Core.Model.SaveStrategies;
using Newtonsoft.Json.Linq;
 
namespace EasySave.Core.Model;

using System.Diagnostics;

public class JobExecutorModel
{
    private Dictionary<SaveType, ISaveStrategy> _strategy;
    private List<string> _prioritizedExtensions;
    private Dictionary<string, ISaveStrategy> _activeJobs;

    public JobExecutorModel()
    {
        _strategy = new Dictionary<SaveType, ISaveStrategy>
        {
            { SaveType.Full, new FullSaveStrategy() },
            { SaveType.Differential, new DifferentialSaveStrategy() },
        };
        _prioritizedExtensions = new();
        GetPrioritizedExtensions();

        _activeJobs = new Dictionary<string, ISaveStrategy>();
    }

    public void Save(Job job, ManualResetEventSlim _pauseEvent, ManualResetEventSlim _stopEvent)
    {
        if (_strategy.TryGetValue(job.saveType, out ISaveStrategy selectedSaveStrat)) // Get the right save strategy
        {
            Stopwatch chrono = new Stopwatch();
            chrono.Start();
            int zero = 0;

            _activeJobs[job.name] = selectedSaveStrat; // associate name to strat

            try
            {
                selectedSaveStrat.Save(job, -1,job, chrono, ref zero, _pauseEvent,_stopEvent );
            }
            catch (Exception ex)
            {
                throw new Exception("Business Software is running, save is self-stopped by caution", ex);
            }
            finally
            {
                _activeJobs.Remove(job.name);
            }
        }
    }

    public void GetPrioritizedExtensions()
    {
        string filePath = "config.json";

        if (!File.Exists(filePath))
            return;

        string json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Console.WriteLine("JSON is empty");
            _prioritizedExtensions = new List<string>();
            return;
        }

        JObject jsonObj = JObject.Parse(json);
        _prioritizedExtensions = jsonObj["ExtensionsPriority"]?.ToObject<List<string>>() ?? new List<string>();
    }
    public List<List<Job>> CalculatePrioScore(List<Job> jobs)
    {
        List<JobPriorizationWrapper> prioritizedJobs = new List<JobPriorizationWrapper>();

        foreach (Job job in jobs)
        {
            int prioScore = 0;
            string[] files = Directory.GetFiles(job.sourcePath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string extension = Path.GetExtension(file);
                if (_prioritizedExtensions.Contains(extension))
                {
                    prioScore++;
                }
            }

            prioritizedJobs.Add(new JobPriorizationWrapper
            {
                prioScore = prioScore,
                job = job
            });
        }

        var sortedJobs = prioritizedJobs.OrderByDescending(j => j.prioScore).ToList();

        List<Job> highPriorityJobs = sortedJobs.Where(j => j.prioScore > 0).Select(j => j.job).ToList();
        List<Job> lowPriorityJobs = sortedJobs.Where(j => j.prioScore == 0).Select(j => j.job).ToList();

        return new List<List<Job>> { highPriorityJobs, lowPriorityJobs };
    }

}

