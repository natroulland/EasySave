using EasySave.View.Cli.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace EasySave.View.Cli;

public class Cli
{
    private CommandApp _app;

    public Cli()
    {
        _app = new CommandApp();
        _app.Configure(config =>
        {
            config.AddCommand<Manualsave>("save");
            config.AddCommand<ListJob>("ls");
            config.AddCommand<ExecuteJob>("exjob");
            config.AddCommand<ExecuteMultipleJob>("exmultjobs");
        });
    }

    public int Run(string[] args)
    {
        return _app.Run(args);
    }
}
