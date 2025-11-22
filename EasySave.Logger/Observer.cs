using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Logger
{
    public abstract class Observer // Observer pattern in order to update the log file
    {
        public abstract void Update(Dictionary<string, string> data);
    }
}
