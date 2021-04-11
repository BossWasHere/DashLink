using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DashLink.Spotify.Net.Model
{
    public class MediaAlbum
    {
        [JsonPropertyName("external_urls")]
        public Dictionary<string, string> ExternalUrls { get; set; }
        [JsonPropertyName("album_type")]
        public string AlbumType { get; set; }
        [JsonPropertyName("artists")]
        public List<ArtistEntry> Artists { get; set; }
        [JsonPropertyName("available_markets")]
        public List<string> AvailableMarkets { get; set; }
        [JsonPropertyName("href")]
        public string RefUrl { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }
        [JsonPropertyName("release_date_precision")]
        public string ReleaseDatePrecision { get; set; }
        [JsonPropertyName("total_tracks")]
        public int TotalTracks { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("uri")]
        public string SpotifyUri { get; set; }
    }
}
