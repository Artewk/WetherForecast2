using System;
using System.Text.Json;
using WetherForecast2.Models;

namespace WetherForecast2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string json = """
            {
              "daily": {
                "time": ["2026-10-16", "2026-10-17"],
                "temperature_2m_max": [12.1, 11.4],
                "temperature_2m_min": [4.3, 3.9],
                "precipitation_sum": [0.0, 1.2]
              }
            }
            """;

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            ForecastResponse forecast = JsonSerializer.Deserialize<ForecastResponse>(json, options);

            Console.WriteLine("Перший день: " + forecast.Daily.Time[0]);
            Console.WriteLine("Максимум: " + forecast.Daily.TempMax[0]);
            Console.WriteLine("Кількість днів: " + forecast.Daily.Time.Count);
        }
    }
}