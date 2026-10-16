using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WetherForecast2.Models;
using System.Globalization;

namespace WetherForecast2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Write("Введіть місто: ");
            string city = Console.ReadLine();

            HttpClient client = new HttpClient();

            string url = "https://geocoding-api.open-meteo.com/v1/search?name="
                + Uri.EscapeDataString(city) + "&count=1&language=uk";

            string json = await client.GetStringAsync(url);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            GeoResponse geo = JsonSerializer.Deserialize<GeoResponse>(json, options);

            if (geo.Results == null || geo.Results.Count == 0)
            {
                Console.WriteLine("Місто не знайдено.");
                return;
            }

            GeoResult place = geo.Results[0];

            Console.WriteLine("Знайдено: " + place.Name + ", " + place.Country);
            Console.WriteLine("Широта: " + place.Latitude);
            Console.WriteLine("Довгота: " + place.Longitude);

            //Перетворюємо координати в текст з крапкою, а не комою
            string lat = place.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = place.Longitude.ToString(CultureInfo.InvariantCulture);

            string forecastUrl = "https://api.open-meteo.com/v1/forecast?latitude=" + lat
                + "&longitude=" + lon
                + "&daily=temperature_2m_max,temperature_2m_min,precipitation_sum"
                + "&forecast_days=5&timezone=auto";

            string forecastJson = await client.GetStringAsync(forecastUrl);

            ForecastResponse forecast = JsonSerializer.Deserialize<ForecastResponse>(forecastJson, options);

            Console.WriteLine();
            Console.WriteLine("Прогноз на " + forecast.Daily.Time.Count + " днів:");

            for (int i = 0; i < forecast.Daily.Time.Count; i++)
            {
                Console.WriteLine(forecast.Daily.Time[i]
                    + ": мін " + forecast.Daily.TempMin[i]
                    + ", макс " + forecast.Daily.TempMax[i]
                    + ", опади " + forecast.Daily.Precipitation[i]);
            }
        }
    }
}