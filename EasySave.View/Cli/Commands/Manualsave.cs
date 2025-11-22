using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.Core.Model;
using Spectre.Console;
using Spectre.Console.Cli;
using EasySave.ViewModel;
using EasySave.Core.Model.Enums;
using EasySave.Core.Model.Entities;

namespace EasySave.View.Cli.Commands;

public class Manualsave : Command<Manualsave.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-s|--source <PATH>")]
        [Description("Source folder path")]
        public string srcPath { get; set; }

        [CommandOption("-d|--destination <PATH>")]
        [Description("Destination folder path")]
        public string destPath { get; set; }

        [CommandOption("-t|--type <TYPE>")]
        [Description("Type of save: 0 = Full, 1 = Differential")]
        public int type { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (settings.type == 0 || settings.type == 1)
        {
            SaveType type = SaveType.Full;
            switch (settings.type)
            {
                case 0:
                    type = SaveType.Full;
                    break;

                case 1:
                    type = SaveType.Differential;
                    break;
            }

            JobExecutorController jobExecutorController = new();
            Job jobToExecute = new Job
            {
                name = "manual job",
                sourcePath = settings.srcPath,
                targetPath = settings.destPath,
                saveType = type
            };
            List<Job> jobs = new List<Job> { jobToExecute };
            jobExecutorController.ExecuteJob(jobs);
            return 0;
        } else
        {
            AnsiConsole.MarkupLine("[red]Please specify a valid type of save[/]");
            return 1;
        }
    }
}