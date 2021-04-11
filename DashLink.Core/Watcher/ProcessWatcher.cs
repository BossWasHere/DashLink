using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Core.Watcher
{
    public class ProcessWatcher
    {
        //private const string QueryString = "SELECT Name, ProcessId, Caption, ExecutablePath FROM Win32_Process";
        private const string StartEventQuery = "SELECT * FROM Win32_ProcessStartTrace";
        private const string StopEventQuery = "SELECT * FROM Win32_ProcessStopTrace";

        private ManagementEventWatcher startWatcher, stopWatcher;
        private bool started = false;

        private readonly ICollection<string> watch;

        public delegate void ProcessEventHandler(object sender, ProcessEventArgs args);
        public event ProcessEventHandler OnProcessEvent;

        public ProcessWatcher(ICollection<string> watch)
        {
            this.watch = watch;
        }

        ~ProcessWatcher()
        {
            EndWatch();
        }

		/*
		 * Info from:
		 * https://www.codeproject.com/Articles/12138/Process-Information-and-Notifications-using-WMI
		 * https://stackoverflow.com/questions/8455873/how-to-detect-a-process-start-end-using-c-sharp-in-windows
		 * https://stackoverflow.com/questions/972039/is-there-a-system-event-when-processes-are-created
		 * https://stackoverflow.com/questions/262280/how-can-i-know-if-a-process-is-running
		 */

		public void ScanAll()
        {
			var processes = Process.GetProcesses();
			foreach (var process in processes)
            {
				if (watch.Contains(process.ProcessName))
                {
					OnProcessEvent?.Invoke(this, new ProcessEventArgs(process.Id, process.ProcessName, true));
                }
            }
        }

		public void BeginWatch()
        {
            if (started) return;
            ScanAll();

            if (startWatcher == null)
            {
                WqlEventQuery weq = new WqlEventQuery(StartEventQuery);
                startWatcher = new ManagementEventWatcher(weq);

                startWatcher.EventArrived += StartMew_EventArrived;
            }

            startWatcher.Start();

            if (stopWatcher == null)
            {
                WqlEventQuery weq = new WqlEventQuery(StopEventQuery);
                stopWatcher = new ManagementEventWatcher(weq);

                stopWatcher.EventArrived += StopMew_EventArrived;
            }

            stopWatcher.Start();
        }

        public void EndWatch()
        {
            if (started)
            {
                started = false;
                startWatcher?.Stop();
                stopWatcher?.Stop();
            }
        }

        private void StartMew_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var pid = (int)e.NewEvent.Properties["ProcessID"].Value;
            var processName = (string)e.NewEvent.Properties["ProcessName"].Value;

            if (watch.Contains(processName))
            {
                OnProcessEvent?.Invoke(this, new ProcessEventArgs(pid, processName, true));
            }
        }

        private void StopMew_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var pid = (int)e.NewEvent.Properties["ProcessID"].Value;
            var processName = (string)e.NewEvent.Properties["ProcessName"].Value;

            if (watch.Contains(processName))
            {
                OnProcessEvent?.Invoke(this, new ProcessEventArgs(pid, processName, false));
            }
        }
    }
}
