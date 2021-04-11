using DashLink.Core;
using DashLink.Core.Action;
using DashLink.Core.Data;
using DashLink.Spotify.Legacy;
using DashLink.Spotify.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DashLink.Spotify
{
    public sealed class SpotifyModule : IModule
    {
        private const int MIN_PLAYBACK_STATE_DELAY = 500;

        public string Name => SpotifyTask.ROOT;

        public event ModuleMessageHandler ModuleMessageEvent;
        public SpotifyConfig Config { get; set; }
        public BaseOAuthClient SpotifyClient { get; set; }

        public bool IsUsingLegacy => Config.LegacyMode || SpotifyClient == null;

        public int CurrentVolume { get; private set; }
        public int NonMuteVolume { get; private set; }
        public bool IsPlayerAvailable { get; private set; }
        public bool IsMuted { get; private set; }
        public bool IsPaused { get; private set; }
        public bool ShuffleEnabled { get; private set; }
        public RepeatMode RepeatMode { get; private set; }

        public bool IsEpisode { get; private set; }
        public string CurrentlyPlayingName { get; private set; }
        public string CurrentlyPlayingArtist { get; private set; }
        public string CurrentlyPlayingAlbum { get; private set; }

        public int CurrentDuration { get; private set; }
        public int CurrentProgress
        {
            get
            {
                int elapsed = lastSongPlaybackPosition + (int)((positionOffsetTime > -1 ? positionOffsetTime : TickReference()) - fetchPlaybackTime - pauseCumulative);
                return Math.Min(elapsed, CurrentDuration);
            }
        }

        private bool shouldFetch = true;
        private long fetchPlaybackTime = -1, positionOffsetTime = -1, pauseCumulative = 0;
        private int lastSongPlaybackPosition;

        public SpotifyModule()
        { }

        public SpotifyModule(SpotifyConfig config)
        {
            Config = config;
            NonMuteVolume = -1;
        }

        public SpotifyModule(SpotifyConfig config, BaseOAuthClient spotifyClient) : this(config)
        {
            SpotifyClient = spotifyClient;
            NonMuteVolume = -1;
        }

        public void Start(DashLinkHost host)
        {
            if (Config == null)
            {
                Config = SpotifyConfig.LoadDefaults();

                var modConfig = host.Config.ModuleSettings;
                if (modConfig != null)
                {
                    Config.LegacyMode = modConfig.TryGetValue("spotify.legacyMode", out JsonElement val) ? val.GetBoolean() : Config.LegacyMode;
                    Config.VolumeChangePercent = modConfig.TryGetValue("spotify.volumeChangePercent", out val) ? val.GetInt32() : Config.VolumeChangePercent;
                    Config.SeekTime = modConfig.TryGetValue("spotify.seekTime", out val) ? val.GetSingle() : Config.SeekTime;
                    Config.SeekOffset = modConfig.TryGetValue("spotify.seekOffset", out val) ? val.GetInt32() : Config.SeekOffset;
                    Config.PlaybackPositionPollRate = modConfig.TryGetValue("spotify.playbackPositionPollRate", out val) ? val.GetInt32() : Config.PlaybackPositionPollRate;
                    Config.TimeFormat = modConfig.TryGetValue("spotify.timeFormat", out val) ? val.GetString() : Config.TimeFormat;
                }
                else
                {
                    SendMessage("No module settings found - using defaults", ModuleMessageLevel.Warning);
                }
            }
        }

        public void Stop()
        { }

        public void RegisterActions(ActionExecutionRegister aer)
        {
            SpotifyTask.CreateTaskIndex(this, aer);
        }

        public async Task CheckCaptureState()
        {
            if (IsUsingLegacy) return;

            // Check immediately, even if the player has been paused previously
            if (shouldFetch || TickReference() - fetchPlaybackTime > CurrentDuration - lastSongPlaybackPosition) await RecapturePlayerState();
        }

        public async Task RecapturePlayerState()
        {
            if (IsUsingLegacy) return;

            if (TickReference() - fetchPlaybackTime > MIN_PLAYBACK_STATE_DELAY)
            {
                SendMessage("Capturing Spotify player state now", ModuleMessageLevel.Debug);
                var response = await SpotifyClient.Get(EndpointBuilder.GetCurrentPlaybackInformation(null, true), true);
                if (!response.IsSuccessStatusCode)
                {
                    SendMessage("Could not get player status from Spotify API - Status: " + response.StatusCode, ModuleMessageLevel.Error);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    IsPlayerAvailable = false;
                    return;
                }
                IsPlayerAvailable = true;

                var data = await response.Content.ReadAsStringAsync();

                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(data))
                    {
                        var root = doc.RootElement;
                        var item = root.GetProperty("item");
                        var device = root.GetProperty("device");

                        IsEpisode = item.GetProperty("type").GetString() == "episode";
                        if (IsEpisode)
                        {
                            CurrentlyPlayingAlbum = item.GetProperty("show").GetProperty("name").GetString();
                            CurrentlyPlayingArtist = item.GetProperty("description").GetString();
                        }
                        else
                        {
                            CurrentlyPlayingAlbum = item.GetProperty("album").GetProperty("name").GetString();
                            CurrentlyPlayingArtist = item.GetProperty("artists").EnumerateArray().First().GetProperty("name").GetString();
                        }

                        CurrentlyPlayingName = item.GetProperty("name").GetString();
                        CurrentDuration = item.GetProperty("duration_ms").GetInt32();

                        CurrentVolume = device.GetProperty("volume_percent").GetInt32();
                        NonMuteVolume = NonMuteVolume == -1 || CurrentVolume != 0 ? CurrentVolume : NonMuteVolume;

                        IsMuted = CurrentVolume == 0;
                        IsPaused = !root.GetProperty("is_playing").GetBoolean();
                        ShuffleEnabled = root.GetProperty("shuffle_state").GetBoolean();
                        RepeatMode = (RepeatMode)Enum.Parse(typeof(RepeatMode), root.GetProperty("repeat_state").GetString(), true);

                        lastSongPlaybackPosition = root.GetProperty("progress_ms").GetInt32();
                    }
                }
                catch (Exception e)
                {
                    ModuleMessageEvent?.Invoke(this, new ModuleMessageEventArgs(this, ModuleMessageLevel.Error, "Could not parse received JSON playback data", e));
                }

                positionOffsetTime = -1;
                pauseCumulative = 0;
                fetchPlaybackTime = TickReference();
            }
            shouldFetch = false;
            return;
        }

        public string GetDuration()
        {
            return TimeSpan.FromMilliseconds(CurrentDuration).ToString(Config.SafeTimeFormat());
        }

        public string GetProgress()
        {
            return TimeSpan.FromMilliseconds(CurrentProgress).ToString(Config.SafeTimeFormat());
        }

        public async Task<bool> DecreaseVolume(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                MediaHotkeyUtil.DecreaseGlobalVolume();
                return true;
            }
            await CheckCaptureState();
            if (!IsPlayerAvailable) return true;
            int x = task.TimesToExecute;

            // Mute check
            if (CurrentVolume == 0) return true;
            IsMuted = false;

            CurrentVolume = Math.Max(0, CurrentVolume - (x * Config.VolumeChangePercent));
            NonMuteVolume = CurrentVolume;
            return (await SpotifyClient.Put(EndpointBuilder.SetVolume(CurrentVolume), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> IncreaseVolume(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                MediaHotkeyUtil.IncreaseGlobalVolume();
                return true;
            }
            await CheckCaptureState();
            if (!IsPlayerAvailable) return true;
            int x = task.TimesToExecute;

            // Mute check
            if (CurrentVolume == 0) CurrentVolume = NonMuteVolume;
            IsMuted = false;

            CurrentVolume = Math.Min(100, CurrentVolume + (x * Config.VolumeChangePercent));
            NonMuteVolume = CurrentVolume;
            return (await SpotifyClient.Put(EndpointBuilder.SetVolume(CurrentVolume), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> ToggleMute(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                MediaHotkeyUtil.ToggleGlobalVolumeMute();
                return true;
            }
            if (task.TimesToExecute % 2 == 0) return true;
            await CheckCaptureState();
            if (!IsPlayerAvailable) return true;

            IsMuted = !IsMuted;
            if (IsMuted)
            {
                return (await SpotifyClient.Put(EndpointBuilder.SetVolume(0), null, true)).IsSuccessStatusCode;
            }
            return (await SpotifyClient.Put(EndpointBuilder.SetVolume(NonMuteVolume), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> ToggleRepeat(SpotifyTask task)
        {
            if (IsUsingLegacy) return false;

            int x = task.TimesToExecute % 3;
            if (x == 0) return true;
            await CheckCaptureState();
            if (!IsPlayerAvailable) return true;

            switch (RepeatMode)
            {
                case RepeatMode.Off:
                    RepeatMode = x == 1 ? RepeatMode.Track : RepeatMode.Context;
                    break;
                case RepeatMode.Track:
                    RepeatMode = x == 1 ? RepeatMode.Context : RepeatMode.Off;
                    break;
                default:
                    RepeatMode = x == 1 ? RepeatMode.Off : RepeatMode.Track;
                    break;
            }

            return (await SpotifyClient.Put(EndpointBuilder.SetRepeatMode(RepeatMode), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> ToggleShuffle(SpotifyTask task)
        {
            if (IsUsingLegacy) return false;

            if (task.TimesToExecute % 2 == 0) return true;
            await CheckCaptureState();
            if (!IsPlayerAvailable) return true;

            ShuffleEnabled = !ShuffleEnabled;

            return (await SpotifyClient.Put(EndpointBuilder.SetShuffleState(ShuffleEnabled), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> TogglePause(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                MediaHotkeyUtil.TogglePlaybackState();
                return true;
            }
            // We can only check if they toggled twice
            if (task.TimesToExecute % 2 == 0) return true;
            await CheckCaptureState();
            if (!IsPlayerAvailable) return true;

            return await (IsPaused ? Unpause(task) : Pause(task));
        }

        public async Task<bool> Unpause(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                // Warning: Cannot check player state so toggles it instead
                MediaHotkeyUtil.TogglePlaybackState();
                return true;
            }
            if (!IsPlayerAvailable) return true;
            // Actually checking if the same input was received twice is not useful
            IsPaused = false;
            if (positionOffsetTime > -1)
            {
                pauseCumulative += TickReference() - positionOffsetTime;
                positionOffsetTime = -1;
            }

            // Skip capture checks, we know we want to resume
            return (await SpotifyClient.Put(EndpointBuilder.StartOrResumePlayback(), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> Pause(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                // Warning: Cannot check player state so toggles it instead
                MediaHotkeyUtil.TogglePlaybackState();
                return true;
            }
            if (!IsPlayerAvailable) return true;

            // TOOD: Check this actually works
            // Check if we are about to pause, so we can work out the actual playback position later
            if (!IsPaused)
            {
                positionOffsetTime = TickReference();
            }

            // Actually checking if the same input was received twice is not useful
            IsPaused = true;

            // Skip capture checks, we know we want to pause
            return (await SpotifyClient.Put(EndpointBuilder.PausePlayback(), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> SeekForward(SpotifyTask task)
        {
            if (IsUsingLegacy) return false;

            // Don't do this if we are about to recapture anyway
            //await CheckCaptureState();

            // Check song still playing, else update
            if (CurrentProgress == CurrentDuration)
            {
                await RecapturePlayerState();
            }
            if (!IsPlayerAvailable) return true;

            lastSongPlaybackPosition = Math.Min(CurrentDuration, CurrentProgress + (int)(Config.SeekTime * task.TimesToExecute * 1000));
            positionOffsetTime = IsPaused ? TickReference() : -1;
            pauseCumulative = 0;

            return (await SpotifyClient.Put(EndpointBuilder.SeekToPositionInTrack(lastSongPlaybackPosition), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> SeekBackward(SpotifyTask task)
        {
            if (IsUsingLegacy) return false;

            // Don't do thius if we are about to recapture anyway
            //await CheckCaptureState();

            // Check song still playing, else update
            if (CurrentProgress == CurrentDuration)
            {
                await RecapturePlayerState();
            }
            if (!IsPlayerAvailable) return true;

            lastSongPlaybackPosition = Math.Max(0, CurrentProgress - (int)(Config.SeekTime * task.TimesToExecute * 1000));
            positionOffsetTime = IsPaused ? TickReference() : -1;
            pauseCumulative = 0;

            return (await SpotifyClient.Put(EndpointBuilder.SeekToPositionInTrack(lastSongPlaybackPosition), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> RestartTrack(SpotifyTask task)
        {
            if (IsUsingLegacy) return false;

            // Actually checking if the same input was received twice is not useful

            if (!IsPlayerAvailable) return true;
            // Notifies us that we should get the current player data next time
            shouldFetch = true;

            return (await SpotifyClient.Put(EndpointBuilder.SeekToPositionInTrack(0), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> SkipForward(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                MediaHotkeyUtil.SkipNextTrack();
                return true;
            }

            // Actually checking if the same input was received twice is not useful

            if (!IsPlayerAvailable) return true;
            // Notifies us that we should get the current player data next time
            shouldFetch = true;

            return (await SpotifyClient.Post(EndpointBuilder.SkipToNextTrack(), null, true)).IsSuccessStatusCode;
        }

        public async Task<bool> SkipBackward(SpotifyTask task)
        {
            if (IsUsingLegacy)
            {
                MediaHotkeyUtil.SkipPreviousTrack();
                return true;
            }

            // Actually checking if the same input was received twice is not useful

            if (!IsPlayerAvailable) return true;
            // Notifies us that we should get the current player data next time
            shouldFetch = true;

            return (await SpotifyClient.Post(EndpointBuilder.SkipToLastTrack(), null, true)).IsSuccessStatusCode;
        }

        private long TickReference()
        {
            return Environment.TickCount;
        }

        private void SendMessage(string message, ModuleMessageLevel level = ModuleMessageLevel.Info)
        {
            ModuleMessageEvent?.Invoke(this, new ModuleMessageEventArgs(this, level, message));
        }
    }
}
