using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DashLink.Core.Config
{
    public class Binding
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }
        [JsonPropertyName("commands")]
        public List<string> Commands { get; set; }
    }
}
