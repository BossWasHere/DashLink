using DashLink.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DashLink.Core.Config
{
    public class Command
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("desc")]
        public string Description { get; set; }
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CommandType Type { get; set; }
        [JsonPropertyName("exec")]
        public CommandExec Execute { get; set; }
    }

    public class CommandExec
    {

        [JsonPropertyName("run")]
        public string Run { get; set; }
        [JsonPropertyName("args")]
        public string Args { get; set; }
        [JsonPropertyName("startIn")]
        public string StartIn { get; set; }
    }
}
