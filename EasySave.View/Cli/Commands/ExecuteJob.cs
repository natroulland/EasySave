using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using EasySave.ViewModel;
using EasySave.View.MenuStrategies;
using System.Text.Json;
using EasySave.Core.Model.Entities;

namespace EasySave.View.Cli.Commands;

public class ExecuteJob : Command<ExecuteJob.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<ID>")]
        [Description("ID of the job (use ls command to list the available jobs)")]
        public int individualID { get; set; }
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

        var indexedJobs = availableJobs.Select(
            (job, index) => new
            {
                Index = index + 1,
                Job = job
            }
            ).ToList();

        if (settings.individualID == null)
        {
            AnsiConsole.MarkupLine("[red]Please specify a valid job ID (integer)[/]");
            return 1;
        }
        else
        {
            List<Job> selectedJobs = new List<Job>();

            var jobToAdd = indexedJobs.FirstOrDefault(entry => entry.Index == settings.individualID);

            if (jobToAdd == null)
            {
                AnsiConsole.MarkupLine("[red]No job found with this ID[/]");
                return 1;
            }
            selectedJobs.Add(jobToAdd.Job);
            jobExecutorController.ExecuteJob(selectedJobs);
            return 0;
        }
    }
}



