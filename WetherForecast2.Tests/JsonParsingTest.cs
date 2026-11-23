using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using WetherForecast2.Models;
using Xunit;

namespace WetherForecast2.Tests
{
    public class JsonParsingTests
    {
        private JsonSerializerOptions CreateOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            return options;
        }

        [Fact]
        public void Forecast_ParsesDailyValues()
        {
            string json = """
            {
              "daily": {
                "time": ["2026-11-21", "2026-11-22"],
                "temperature_2m_max": [12.1, 11.4],
                "temperature_2m_min": [4.3, 3.9],
                "precipitation_sum": [0.0, 1.2]
              }
            }
            """;

            ForecastResponse forecast = JsonSerializer.Deserialize<ForecastResponse>(json, CreateOptions());

            Assert.Equal(2, forecast.Daily.Time.Count);
            Assert.Equal("2026-11-21", forecast.Daily.Time[0]);
            Assert.Equal(12.1, forecast.Daily.TempMax[0]);
            Assert.Equal(3.9, forecast.Daily.TempMin[1]);
            Assert.Equal(1.2, forecast.Daily.Precipitation[1]);
        }

        [Fact]
        public void Geo_ParsesCity()
        {
            string json = """
            {
              "results": [
                { "name": "Луцьк", "country": "Україна", "latitude": 50.75784, "longitude": 25.35024 }
              ]
            }
            """;

            GeoResponse geo = JsonSerializer.Deserialize<GeoResponse>(json, CreateOptions());

            Assert.Single(geo.Results);
            Assert.Equal("Луцьк", geo.Results[0].Name);
            Assert.Equal(50.75784, geo.Results[0].Latitude);
        }

        [Fact]
        public void Geo_CityNotFound_ResultsIsNull()
        {
            // Коли міста не існує, у відповіді API немає ключа "results"
            string json = "{}";

            GeoResponse geo = JsonSerializer.Deserialize<GeoResponse>(json, CreateOptions());

            Assert.Null(geo.Results);
        }

        [Fact]
        public void InvalidJson_ThrowsJsonException()
        {
            string json = "це не json";

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<ForecastResponse>(json, CreateOptions()));
        }

        [Fact]
        public void History_SerializeAndDeserialize_KeepsData()
        {
            List<HistoryEntry> original = new List<HistoryEntry>();
            HistoryEntry entry = new HistoryEntry();
            entry.City = "Львів";
            entry.Country = "Україна";
            entry.Date = "2026-11-21 12:00";
            original.Add(entry);

            string json = JsonSerializer.Serialize(original);
            List<HistoryEntry> loaded = JsonSerializer.Deserialize<List<HistoryEntry>>(json);

            Assert.Single(loaded);
            Assert.Equal("Львів", loaded[0].City);
            Assert.Equal("2026-11-21 12:00", loaded[0].Date);
        }
    }
}