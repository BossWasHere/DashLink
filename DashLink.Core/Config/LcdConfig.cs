using DashLink.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DashLink.Core.Config
{
    public class LcdConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enable { get; set; }
        [JsonPropertyName("poll")]
        public int Poll { get; set; }
        [JsonPropertyName("top")]
        public LcdLine Top { get; set; }
        [JsonPropertyName("bottom")]
        public LcdLine Bottom { get; set; }

        public static LcdConfig EmptyConfig()
        {
            return new LcdConfig()
            {
                Enable = false,
                Poll = 0,
                Top = new LcdLine()
                {
                    Source = LcdLineSource.None
                },
                Bottom = new LcdLine()
                {
                    Source = LcdLineSource.None
                }
            };
        }
    }

    public class LcdLine
    {
        [JsonPropertyName("source")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LcdLineSource Source { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("overflow")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LcdLineOverflow Overflow { get; set; }
    }
}
