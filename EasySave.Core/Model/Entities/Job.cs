using EasySave.Core.Model.Enums;

namespace EasySave.Core.Model.Entities;

public class Job
{
    /// <summary>
    /// Name of the job
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Source of the save
    /// </summary>
    public string sourcePath { get; set; }
    /// <summary>
    /// Destination of the save
    /// </summary>
    public string targetPath { get; set; }
    /// <summary>
    /// Type of the save
    /// </summary>
    public SaveType saveType { get; set; }

    public override string ToString()
    {
        return $"{name} - {sourcePath} -> {targetPath} ({saveType})";
    }
}