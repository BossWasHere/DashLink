using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Watcher
{
    public class ProcessEventArgs : EventArgs
    {
        public int PID { get; set; }
        public string ProcessName { get; set; }
        public bool IsRunning { get; set; }

        public ProcessEventArgs(int pid, string processName, bool isRunning)
        {
            PID = pid;
            ProcessName = processName;
            IsRunning = isRunning;
        }
    }
}
