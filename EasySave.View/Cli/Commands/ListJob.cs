using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using EasySave.Core.Model.Enums;
using EasySave.Core.Model.Entities;
using EasySave.Core.Model;
using EasySave.ViewModel;
using EasySave.View.Menus;
using EasySave.View.MenuStrategies;
using System.Text.Json;

namespace EasySave.View.Cli.Commands;

public class ListJob : Command<ListJob.Settings>
{
    public class Settings : CommandSettings
    {
        // Leave blank
    }

    public override int Execute(CommandContext context, Settings settings)
    {
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

        var indexedJobs = availableJobs.Select(
            (job, index) => new { 
                Index = index + 1,
                Job = job }
            ).ToList();

        foreach (var job in indexedJobs)
        {
            AnsiConsole.MarkupLine($"ID: {job.Index} | {job.Job.ToString()}");
        }
        return 0;
    }
}
