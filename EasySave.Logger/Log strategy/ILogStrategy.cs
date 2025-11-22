using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Logger.Log_strategy
{
    public interface ILogStrategy
    {
        void WriteLog(string filePath, string content);
    }

}
