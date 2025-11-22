using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.View.MenuStrategies;
using Spectre.Console;
using EasySave.ViewModel;
using EasySave.Core.Model.Entities;
using System.Text.Json;
using EasySave.View.Languages;

namespace EasySave.View.Menus;

public class JobExecutorViewStrategy : IMenuStrategy
{
    private JobExecutorController jobExecutorController;
    private List<Job> jobs;
    private Language _translations { get; set; }

    public JobExecutorViewStrategy(Language translations)
    {
        jobExecutorController = new();
        _translations = translations;
        GetJobsFromJson();
    }
    /// <summary>
    /// Display the job executor menu 
    /// </summary>
    /// <returns></returns>
    public string Display()
    {
        GetJobsFromJson();

        if (!jobs.Any())
        {
            AnsiConsole.WriteLine(_translations.GetStringFromJson("jobExecutorNoJobs"));
            Console.ReadKey();

            return "";
        }

        List<string> selectedJobNames = AnsiConsole.Prompt( // Remplacer par List<Job> 
            new MultiSelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("jobExecutorSelect"))
                .PageSize(10)
                .MoreChoicesText(_translations.GetStringFromJson("jobExecutorMoreChoices"))
                .InstructionsText(_translations.GetStringFromJson("jobExecutorInstructions"))
                .AddChoices(jobs.Select(job => job.ToString()))
                );

        List<Job> selectedJobs = jobs.Where(job => selectedJobNames.Contains(job.ToString())).ToList();

        executeJob(selectedJobs);

        return "";
    }

    public void executeJob(List<Job> selectedJob)
    {
        jobExecutorController.ExecuteJob(selectedJob);
    }
    /// <summary>
    /// Retrieve jobs from the JSON file
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void GetJobsFromJson()
    {
        string json = File.ReadAllText("./config.json");

        if (string.IsNullOrWhiteSpace(json))
        {
           Console.WriteLine("The JSON file is empty");
            return;
        }

        var jobListWrapper = JsonSerializer.Deserialize<JobListWrapper>(json);

        if (jobListWrapper == null || jobListWrapper.Jobs == null)
        {
            throw new Exception("Deserialization error");
        }

        jobs = jobListWrapper.Jobs;
    }
}

