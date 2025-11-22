using EasySave.Core.Model.Entities;
using System.Diagnostics;

public interface ISaveStrategy
{
    void Save(Job job, int lasttimeupdate, Job ParentJob, Stopwatch parentchrono, ref int number, ManualResetEventSlim pauseEvent, ManualResetEventSlim stopEvent);
    bool DetectProcess();

}
