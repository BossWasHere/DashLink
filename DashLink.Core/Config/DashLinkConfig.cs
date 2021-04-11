using DashLink.Core.Action;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DashLink.Core.Config
{
    public class DashLinkConfig
    {
        public const string DefaultId = "Default";

        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("defaultProfile")]
        public string DefaultProfile { get; set; }
        [JsonPropertyName("lastProfile")]
        public string LastProfile { get; set; }
        [JsonPropertyName("asyncCommands")]
        public bool AsyncCommands { get; set; }
        [JsonPropertyName("swapRotaryDirection")]
        public bool SwapRotaryDirection { get; set; }

        [JsonPropertyName("moduleSettings")]
        public Dictionary<string, JsonElement> ModuleSettings { get; set; }
        [JsonPropertyName("commandDebounce")]
        public int CommandDebounce { get; set; }


        [JsonPropertyName("commands")]
        public List<Command> Commands { get; set; }
        [JsonPropertyName("profiles")]
        public List<Profile> Profiles { get; set; }

        public static DashLinkConfig EmptyConfig()
        {
            return new DashLinkConfig()
            {
                Version = "1.0",
                DefaultProfile = DefaultId,
                LastProfile = DefaultId,
                SwapRotaryDirection = false,
                ModuleSettings = new Dictionary<string, JsonElement>(),
                Commands = new List<Command>(),
                Profiles = new List<Profile>
                {
                    Profile.EmptyProfile(DefaultId)
                }
            };
        }
    }
}
