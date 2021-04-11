using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net.Model
{
    public class EpisodeMediaItem
    {
        [JsonPropertyName("audio_preview_url")]
        public string AudioPreviewUrl { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("duration_ms")]
        public int DurationMs { get; set; }
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
        [JsonPropertyName("external_urls")]
        public Dictionary<string, string> ExternalUrls { get; set; }
        [JsonPropertyName("href")]
        public string RefUrl { get; set; }
        [JsonPropertyName("html_description")]
        public string HtmlDescription { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; }
        [JsonPropertyName("is_externally_hosted")]
        public bool IsExternallyHosted { get; set; }
        [JsonPropertyName("is_playable")]
        public bool IsPlayable { get; set; }
        [JsonPropertyName("language")]
        public string Language { get; set; }
        [JsonPropertyName("languages")]
        public List<string> Languages { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }
        [JsonPropertyName("release_date_precision")]
        public string ReleaseDatePrecision { get; set; }
        [JsonPropertyName("show")]
        public EpisodeShow Show { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("uri")]
        public string SpotifyUri { get; set; }

    }

}
