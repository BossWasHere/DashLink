using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DashLink.Core.Config
{
    public class Profile
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("modules")]
        public List<string> Modules { get; set; }
        [JsonPropertyName("detect")]
        public List<string> Detect { get; set; }
        [JsonPropertyName("remainAfterAutoSwitch")]
        public bool RemainAfterAutoSwitch { get; set; }
        [JsonPropertyName("lcd")]
        public LcdConfig Lcd { get; set; }
        [JsonPropertyName("bindings")]
        public List<Binding> Bindings { get; set; }


        public static Profile EmptyProfile(string id)
        {
            return new Profile()
            {
                Id = id,
                Modules = new List<string>(),
                Detect = new List<string>(),
                RemainAfterAutoSwitch = false,
                Lcd = LcdConfig.EmptyConfig(),
                Bindings = new List<Binding>()
            };
        }
    }
}
