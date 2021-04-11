using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net.Model
{
    public class CurrentlyPlayingTrackResponse
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        [JsonPropertyName("context")]
        public string Context { get; set; }
        [JsonPropertyName("progress_ms")]
        public int ProgressMs { get; set; }
        [JsonPropertyName("item")]
        public TrackMediaItem Item { get; set; }
        [JsonPropertyName("currently_playing_type")]
        public string CurrentlyPlayingType { get; set; }
        [JsonPropertyName("actions")]
        public string Actions { get; set; }
        [JsonPropertyName("is_playing")]
        public bool IsPlaying { get; set; }
    }
}
