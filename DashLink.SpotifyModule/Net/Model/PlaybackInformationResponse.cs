using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net.Model
{
    public abstract class PlaybackInformationResponse
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        [JsonPropertyName("device")]
        public DeviceInfo Device { get; set; }
        [JsonPropertyName("progress_ms")]
        public int ProgressMs { get; set; }
        [JsonPropertyName("is_playing")]
        public bool IsPlaying { get; set; }
        [JsonPropertyName("currently_playing_type")]
        public string CurrentlyPlayingType { get; set; }
        [JsonPropertyName("shuffle_state")]
        public bool ShuffleState { get; set; }
        [JsonPropertyName("repeat_state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RepeatMode RepeatState { get; set; }
        [JsonPropertyName("context")]
        public dynamic Context { get; set; }
        [JsonPropertyName("actions")]
        public dynamic Actions { get; set; }
    }

    public class TrackPlaybackInformationResponse : PlaybackInformationResponse
    {
        [JsonPropertyName("item")]
        public TrackMediaItem Item { get; set; }

    }

    public class EpisodePlaybackInformationResponse : PlaybackInformationResponse
    {
        [JsonPropertyName("item")]
        public EpisodeMediaItem Item { get; set; }

    }
}
