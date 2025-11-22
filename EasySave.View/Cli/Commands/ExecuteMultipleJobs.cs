using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;
using EasySave.Core.Model;
using EasySave.View.MenuStrategies;
using EasySave.ViewModel;
using EasySave.Core.Model.Entities;

namespace EasySave.View.Cli.Commands;

public class ExecuteMultipleJob : Command<ExecuteMultipleJob.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[RANGE]")]
        [Description("Job IDs to execute, e.g. '1-3'")]
        public string multipleID { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        JobExecutorController jobExecutorController = new();
        List<Job> availableJobs;
        string json = File.ReadAllText("./config.json");

        if (string.IsNullOrWhiteSpace(json))
        {
            AnsiConsole.MarkupLine("[red]No jobs available[/]");
            return 1;
        }

        JobListWrapper jobListWrapper = JsonSerializer.Deserialize<JobListWrapper>(json);
        if (jobListWrapper == null || jobListWrapper.Jobs == null)
        {
            throw new Exception("Deserialization error");
        }
        availableJobs = jobListWrapper.Jobs;

        var indexedJobs = availableJobs
            .Select((job, index) => new { Index = index + 1, Job = job })
            .ToList();

        List<int>? jobIDs = ParseJobRange(settings.multipleID);
        if (jobIDs == null)
        {
            AnsiConsole.MarkupLine("[red]Invalid job ID format. Use '1-3' to select jobs from 1 to 3.[/]");
            return 1;
        }

        List<Job> selectedJobs = new();
        List<int> invalidIDs = new();
        foreach (int id in jobIDs)
        {
            var jobToAdd = indexedJobs.FirstOrDefault(entry => entry.Index == id);
            if (jobToAdd == null)
            {
                invalidIDs.Add(id);
            }
            else
            {
                selectedJobs.Add(jobToAdd.Job);
            }
        }

        if (invalidIDs.Count > 0)
        {
            AnsiConsole.MarkupLine($"[red]No jobs found for the following IDs : {string.Join(", ", invalidIDs)}[/]");
            return 1;
        }

        jobExecutorController.ExecuteJob(selectedJobs);
        return 0;
    }

    /// <summary>
    /// Parse ID range, return list of int.
    /// </summary>
    private List<int>? ParseJobRange(string input)
    {
        string[] parts = input.Split('-');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end))
        {
            return null;
        }

        if (start > end)
        {
            return null;
        }

        return Enumerable.Range(start, end - start + 1).ToList();
    }
}

