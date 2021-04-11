using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net.Model
{
    public class EpisodeShow
    {
        [JsonPropertyName("available_markets")]
        public List<string> AvailableMarkets { get; set; }
        [JsonPropertyName("copyrights")]
        public List<string> Copyrights { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
        [JsonPropertyName("external_urls")]
        public Dictionary<string, string> ExternalUrls { get; set; }
        [JsonPropertyName("href")]
        public string RefUrl { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; }
        [JsonPropertyName("is_externally_hosted")]
        public bool IsExternallyHosted { get; set; }
        [JsonPropertyName("languages")]
        public List<string> Languages { get; set; }
        [JsonPropertyName("media_type")]
        public string MediaType { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("publisher")]
        public string Publisher { get; set; }
        [JsonPropertyName("total_episodes")]
        public int TotalEpisodes { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("uri")]
        public string SpotifyUri { get; set; }

    }
}
