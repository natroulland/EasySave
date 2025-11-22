using EasySave.Core;
using EasySave.ViewModel;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.View;
using EasySave.Core.Model.Enums;
using System.ComponentModel.Design;
using EasySave.View.Languages;
using EasySave.Core.Model.Entities;

namespace EasySave.View.MenuStrategies;

public class JobEditorViewStrategy : IMenuStrategy
{
    private readonly JobManagerViewModel _jobManagerViewModel;
    private Language _translations { get; set; }

    public JobEditorViewStrategy(Language translations)
    {
        _jobManagerViewModel = new JobManagerViewModel();
        _translations = translations;
        GetJobsFromJson();
    }

    public List<Job> Jobs { get; set; } = new();
    /// <summary>
    /// Displays Job Edition menu (Create, Update, Delete, Back to main menu)
    /// </summary>
    /// <returns></returns>
    public string Display()
    {
        string action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("jobEditorMenu"))
                .AddChoices(
                    _translations.GetStringFromJson("jobEditorAdd"),
                    _translations.GetStringFromJson("jobEditorUpdate"),
                    _translations.GetStringFromJson("jobEditorDelete"),
                    _translations.GetStringFromJson("jobEditorBack")
                )
        );

        if (action == _translations.GetStringFromJson("jobEditorAdd"))
        {
            AddJob();
        }
        else if (action == _translations.GetStringFromJson("jobEditorUpdate"))
        {
            UpdateJob();
        }
        else if (action == _translations.GetStringFromJson("jobEditorDelete"))
        {
            DeleteJob();
        }
        else
        {
            return "";
        }
        return "";
    }
    /// <summary>
    /// Add a job to the list of jobs. If the job already exists, display an error message. 
    /// Options are validated in ViewModel. If the job is added, display a success message. If the job is not valid, display an error message.
    /// </summary>
    private void AddJob()
    {
        string name = AnsiConsole.Ask<string>(_translations.GetStringFromJson("jobCreatorName"));
        string sourcePath = AnsiConsole.Ask<string>(_translations.GetStringFromJson("jobCreatorSource"));
        string targetPath = AnsiConsole.Ask<string>(_translations.GetStringFromJson("jobCreatorDest"));
        SaveType saveType = AnsiConsole.Prompt(
            new SelectionPrompt<SaveType>()
                .Title(_translations.GetStringFromJson("jobCreatorType"))
                .AddChoices(SaveType.Full, SaveType.Differential)
        );

        Job? newJob = new()
        {
            name = name,
            sourcePath = sourcePath,
            targetPath = targetPath,
            saveType = (SaveType)saveType
        };

        if (Jobs.Exists(job => job.name == newJob.name))
        {
            AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorErrorName"));
            Thread.Sleep(1500);
            return;
        }

        Jobs.Add(newJob);
        if (_jobManagerViewModel.ValidateAndSaveJob(newJob, "./config.json"))
        {
            AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorDone"));
            Thread.Sleep(1500);
        }
        else
        {
            if(string.IsNullOrEmpty(newJob.name) || newJob == null)
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorErrorFields"));
            }
            else
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorSourceNotExists"));
            }
            Thread.Sleep(1500);
            Jobs.Remove(newJob);
            return;
        }
    }
    /// <summary>
    /// Update a job from the list of jobs. 
    /// If the job does not exist, display an error message. If the job is updated, display a success message.
    /// </summary>
    private void UpdateJob()
    {
        string choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("jobUpdateTitle"))
                .PageSize(10)
                .MoreChoicesText(_translations.GetStringFromJson("jobUpdateMoreChoices"))
                .AddChoices(Jobs.ConvertAll(job => String.Format(
                    _translations.GetStringFromJson("jobUpdateJobs"), job.name, job.sourcePath, job.targetPath, job.saveType)))
                .AddChoices(_translations.GetStringFromJson("jobUpdateExit"))
        );

        if (choix == _translations.GetStringFromJson("jobUpdateExit"))
        {
            return;
        }

        Job? jobToUpdate = Jobs.Find(job => String.Format(_translations.GetStringFromJson("jobUpdateJobs"), job.name, job.sourcePath, job.targetPath, job.saveType) == choix);
        if (jobToUpdate == null)
        {
            AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobUpdateJobNotFound"));
            Thread.Sleep(1500);
            return;
        }

        Job oldJob = jobToUpdate;

        jobToUpdate.name = AnsiConsole.Ask<string>(_translations.GetStringFromJson("jobUpdateNewJobName"), jobToUpdate.name);
        jobToUpdate.sourcePath = AnsiConsole.Ask<string>(_translations.GetStringFromJson("jobUpdateNewSourcePath"), jobToUpdate.sourcePath);
        jobToUpdate.targetPath = AnsiConsole.Ask<string>(_translations.GetStringFromJson("jobUpdateNewTargetPath"), jobToUpdate.targetPath);
        jobToUpdate.saveType = AnsiConsole.Prompt(
            new SelectionPrompt<SaveType>()
                .Title(_translations.GetStringFromJson("jobUpdateNewSaveType"))
                .AddChoices(SaveType.Full, SaveType.Differential)
        );

        JobManagerViewModel viewModel = new();
        if (_jobManagerViewModel.UpdateJob(jobToUpdate, "./config.json", oldJob.name))
        {
            AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorDone"));
            Thread.Sleep(1500);
        }
        else
        {
            if (string.IsNullOrEmpty(jobToUpdate.name) || jobToUpdate == null)
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorErrorFields"));
            }
            else
            {
                AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobCreatorSourceNotExists"));
            }
            Thread.Sleep(1500);
            jobToUpdate = oldJob;
            return;
        }
        AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobUpdateDone"));
        Thread.Sleep(1500);
    }
    /// <summary>
    /// Delete a job from the list of jobs. 
    /// If the job does not exist, display an error message. If the job is deleted, display a success message.
    /// </summary>
    private void DeleteJob()
    {
        string choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_translations.GetStringFromJson("jobDeleteTitle"))
                .PageSize(10)
                .MoreChoicesText(_translations.GetStringFromJson("jobDeleteMoreChoices"))
                .AddChoices(Jobs.ConvertAll(job => job.name))
                .AddChoices(_translations.GetStringFromJson("jobDeleteMainMenu"))
        );

        if (choix == _translations.GetStringFromJson("jobDeleteMainMenu"))
        {
            return;
        }

        Job? jobToDelete = Jobs.Find(job => job.name == choix);
        if (jobToDelete != null)
        {
            _jobManagerViewModel.DeleteJob(jobToDelete, "./config.json");
            Jobs.Remove(jobToDelete);
            AnsiConsole.MarkupLine(_translations.GetStringFromJson("jobDeleteDone"));
            Thread.Sleep(1500);
        }
    }
    /// <summary>
    /// Get jobs from JSON file. 
    /// </summary>
    public void GetJobsFromJson()
    {
        if (!File.Exists("./config.json"))
            return;

        string json = File.ReadAllText("./config.json");
        if (string.IsNullOrWhiteSpace(json))
        {
            Console.WriteLine("JSON is empty");
            return;
        }

        var jsonData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        if (jsonData != null && jsonData.ContainsKey("Jobs"))
        {
            Jobs = JsonSerializer.Deserialize<List<Job>>(jsonData["Jobs"].GetRawText()) ?? new List<Job>();
        }
    }

}

public class JobListWrapper
{
    public List<Job> Jobs { get; set; }
}
