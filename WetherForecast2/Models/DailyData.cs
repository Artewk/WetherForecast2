using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WetherForecast2.Models
{
    internal class DailyData
    {
        public List<string> Time { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double> TempMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double> TempMin { get; set; }

        [JsonPropertyName("precipitation_sum")]
        public List<double> Precipitation { get; set; }
    }
}
