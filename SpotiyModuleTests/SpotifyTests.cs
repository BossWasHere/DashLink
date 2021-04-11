using DashLink.Spotify.Net.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json;

namespace SpotiyModuleTests
{
    [TestClass]
    public class SpotifyTests
    {
        [TestMethod]
        public void JsonParseTrackTest()
        {
            var bytes = File.ReadAllText("./Resource/TrackInformation.json");

            var obj = JsonSerializer.Deserialize<TrackPlaybackInformationResponse>(bytes);

            Assert.AreEqual(obj.ProgressMs, 12345);
            Assert.AreEqual(obj.Device.Name, "PC Name");
            Assert.AreEqual(obj.Item.SpotifyUri, "spotify:track:track_id");
        }

        [TestMethod]
        public void JsonParseEpisodeTest()
        {
            var bytes = File.ReadAllText("./Resource/EpisodeInformation.json");

            var obj = JsonSerializer.Deserialize<EpisodePlaybackInformationResponse>(bytes);

            Assert.AreEqual(obj.Item.Images[0].Width, 640);
            Assert.AreEqual(obj.Item.ReleaseDate, "2020-05-04");
            Assert.AreEqual(obj.Item.DurationMs, 3000000);
        }
    }
}
