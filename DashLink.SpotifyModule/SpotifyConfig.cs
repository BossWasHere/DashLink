using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DashLink.Spotify
{
    public class SpotifyConfig
    {
        public const string DEFAULT_TIME_FORMAT = "m:ss";

        [JsonPropertyName("legacyMode")]
        public bool LegacyMode { get; set; }
        [JsonPropertyName("volumeChangePercent")]
        public int VolumeChangePercent { get; set; }
        [JsonPropertyName("seekTime")]
        public float SeekTime { get; set; }
        [JsonPropertyName("seekOffset")]
        public int SeekOffset { get; set; }
        [JsonPropertyName("playbackPositionPollRate")]
        public int PlaybackPositionPollRate { get; set; }
        [JsonPropertyName("timeFormat")]
        public string TimeFormat { get; set; }

        public static SpotifyConfig LoadDefaults()
        {
            return new SpotifyConfig()
            {
                LegacyMode = false,
                VolumeChangePercent = 5,
                SeekTime = 15.0F,
                SeekOffset = 0,
                PlaybackPositionPollRate = 10,
                TimeFormat = DEFAULT_TIME_FORMAT
            };
        }

        public string SafeTimeFormat()
        {
            return TimeFormat.Replace(":", "\\:");
        }
    }
}
