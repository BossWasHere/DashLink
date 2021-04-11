using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net.Model
{
    public class ArtistEntry
    {
        [JsonPropertyName("external_urls")]
        public Dictionary<string, string> ExternalUrls;
        [JsonPropertyName("href")]
        public string RefUrl { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("uri")]
        public string SpotifyUri { get; set; }
    }
}
