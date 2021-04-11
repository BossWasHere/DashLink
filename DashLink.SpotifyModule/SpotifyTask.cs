using DashLink.Core.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashLink.Spotify
{
    public class SpotifyTask : DelegatedAction
    {
        private const int DEBOUNCE_MILLIS = 1000;
        public const string ROOT = "spotify";

        public delegate Task<bool> SpotifyTaskExecutor(SpotifyTask task);

        public SpotifyTaskExecutor Executor { get; }
        private readonly SpotifyModule module; 

        private SpotifyTask(SpotifyModule module, SpotifyTaskExecutor executor)
        {
            this.module = module;
            Executor = executor;
        }

        internal static void CreateTaskIndex(SpotifyModule module, ActionExecutionRegister aer)
        {
            aer.Register(ROOT + ".toggle.mute", new SpotifyTask(module, module.ToggleMute));
            aer.Register(ROOT + ".toggle.pause", new SpotifyTask(module, module.TogglePause));
            aer.Register(ROOT + ".toggle.repeat", new SpotifyTask(module, module.ToggleRepeat));
            aer.Register(ROOT + ".toggle.shuffle", new SpotifyTask(module, module.ToggleShuffle));
            aer.Register(ROOT + ".seek.back", new SpotifyTask(module, module.SeekBackward));
            aer.Register(ROOT + ".seek.forward", new SpotifyTask(module, module.SeekForward));
            aer.Register(ROOT + ".skip.back", new SpotifyTask(module, module.SkipBackward));
            aer.Register(ROOT + ".skip.forward", new SpotifyTask(module, module.SkipForward));
            aer.Register(ROOT + ".skip.restart", new SpotifyTask(module, module.RestartTrack));
            aer.Register(ROOT + ".volume.down", new SpotifyTask(module, module.DecreaseVolume));
            aer.Register(ROOT + ".volume.up", new SpotifyTask(module, module.IncreaseVolume));
        }

        public override DebounceMode GetDebounceMode()
        {
            return module.IsUsingLegacy ? DebounceMode.None : DebounceMode.CountFromSecond;
        }

        public override int GetDebounceTime()
        {
            return DEBOUNCE_MILLIS;
        }

        public override Task<bool> GetInvokeAction(string data)
        {
            return new Task<bool>(() =>
            {
                return Executor(this).Result;
            });
        }
    }
}
