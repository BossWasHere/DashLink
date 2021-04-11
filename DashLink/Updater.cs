using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DashLink
{
    public class Updater : INotifyPropertyChanged
    {
        private static readonly string UpdateSource = "https://raw.githubusercontent.com/BossWasHere/DashLink/master/version";
        public static readonly Updater Service = new Updater();

        private Task updaterTask = null;
        private Updater()
        { }

        public string Version
        {
            get
            {
                return App.Version;
            }
        }
        public string LatestVersion { get; private set; }
        public bool IsNewerVersion { get; private set; }
        public Uri ReleasePage { get; private set; }
        public bool ShowHyperlink { get; private set; }

        public static Task RunUpdateChecker()
        {
            return Service.updaterTask = Service.updaterTask ?? Task.Factory.StartNew(() => { Service.CheckForUpdates(); });
        }

        public static void WaitUntilFinished()
        {
            if (Service.updaterTask != null) Service.updaterTask.Wait(10000);
        }

        private void CheckForUpdates()
        {
            string[] verParts;
            try
            {
                //verParts = new string[] { "1.0.0.0", "https://github.com" };
                using (var client = new WebClient())
                {
                    verParts = client.DownloadString(UpdateSource).Split(';');
                }
                if (IsNewer(Version, verParts[0]))
                {
                    LatestVersion = verParts[0];
                    IsNewerVersion = true;
                    OnPropertyChanged(nameof(LatestVersion));
                    OnPropertyChanged(nameof(IsNewerVersion));
                    if (verParts.Length > 1)
                    {
                        if (Uri.TryCreate(verParts[1], UriKind.Absolute, out Uri target))
                        {
                            ReleasePage = target;
                            ShowHyperlink = true;
                            OnPropertyChanged(nameof(ReleasePage));
                            OnPropertyChanged(nameof(ShowHyperlink));
                        }
                    }
                }
            }
            catch { }
        }

        public bool IsNewer(string older, string newer)
        {
            int[] a = older.Split('.').Where(x => char.IsDigit(x[0])).Select(x => int.Parse(x)).ToArray();
            int[] b = newer.Split('.').Where(x => char.IsDigit(x[0])).Select(x => int.Parse(x)).ToArray();
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                if (a[i] < b[i])
                {
                    return true;
                }
                if (a[i] > b[i])
                {
                    return false;
                }
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
