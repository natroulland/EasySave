using EasySave.View;
using EasySave.View.Cli;
using System.IO;
using System.Reflection;

class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            View view = new View();
            return 0;
        }
        else
        {
            return new Cli().Run(args);
        }
    }
}
