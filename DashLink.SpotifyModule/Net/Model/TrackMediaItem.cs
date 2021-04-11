using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net.Model
{
    public class TrackMediaItem
    {
        [JsonPropertyName("album")]
        public MediaAlbum Album { get; set; }
        [JsonPropertyName("artists")]
        public List<ArtistEntry> Artists { get; set; }
        [JsonPropertyName("disc_number")]
        public int DiscNumber { get; set; }
        [JsonPropertyName("available_markets")]
        public List<string> AvailableMarkets { get; set; }
        [JsonPropertyName("duration_ms")]
        public int DurationMs { get; set; }
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
        [JsonPropertyName("external_ids")]
        public Dictionary<string, string> ExternalIds { get; set; }
        [JsonPropertyName("external_urls")]
        public Dictionary<string, string> ExternalUrls { get; set; }
        [JsonPropertyName("href")]
        public string RefUrl { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("is_local")]
        public bool IsLocal { get; set; }
        [JsonPropertyName("is_playable")]
        public bool IsPlayable { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }
        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; }
        [JsonPropertyName("track_number")]
        public int TrackNumber { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("uri")]
        public string SpotifyUri { get; set; }
    }

}
