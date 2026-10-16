using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WetherForecast2.Models;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

namespace WetherForecast2
{
    class Program
    {
        static void PrintForecast(GeoResult place, DailyData daily)
        {
            Console.WriteLine();
            Console.WriteLine("Прогноз для: " + place.Name + ", " + place.Country);
            Console.WriteLine();
            Console.WriteLine("Дата".PadRight(12) + "Мін".PadRight(8) + "Макс".PadRight(8) + "Опади, мм");
            Console.WriteLine(new string('-', 38));

            for (int i = 0; i < daily.Time.Count; i++)
            {
                string date = daily.Time[i];
                string min = daily.TempMin[i].ToString("F1");
                string max = daily.TempMax[i].ToString("F1");
                string rain = daily.Precipitation[i].ToString("F1");

                Console.WriteLine(date.PadRight(12) + min.PadRight(8) + max.PadRight(8) + rain);
            }
        }

        static List<HistoryEntry> LoadHistory()
        {
            if (!File.Exists(HistoryFile)) {return new List<HistoryEntry>();}

            try
            {
                string json = File.ReadAllText(HistoryFile);
                List<HistoryEntry> list = JsonSerializer.Deserialize<List<HistoryEntry>>(json);

                if (list == null) {return new List<HistoryEntry>();}

                return list;
            }
            catch (JsonException)
            {
                //Якшо файл зіпсований то починаємо з порожньої історії
                return new List<HistoryEntry>();
            }
        }

        static void SaveHistory(List<HistoryEntry> history)
        {
            JsonSerializerOptions writeOptions = new JsonSerializerOptions();
            writeOptions.WriteIndented = true;
            //Без цього, українські літери в файлі стануть кодами
            writeOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            try
            {
                string json = JsonSerializer.Serialize(history, writeOptions);
                File.WriteAllText(HistoryFile, json);
            }
            catch (IOException)
            {
                Console.WriteLine("Не вдалося зберегти історію у файл.");
            }
        }

        static void ShowHistory(List<HistoryEntry> history)
        {
            if (history.Count == 0)
            {
                Console.WriteLine("Історія порожня.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Останні запити:");

            for (int i = 0; i < history.Count; i++)
            {
                HistoryEntry item = history[i];
                Console.WriteLine((i + 1) + ". " + item.Date + "  " + item.City + ", " + item.Country);
            }
        }

        const string HistoryFile = "history.json";
        static async Task Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Створюємо один раз, а не на кожне коло циклу
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            List<HistoryEntry> history = LoadHistory();

            Console.WriteLine("Програма прогнозу погоди.");
            Console.WriteLine("Команди: історія, вихід");

            while (true)
            {
                Console.WriteLine();
                Console.Write("Введіть місто: ");
                string city = Console.ReadLine();

                if (city == null)
                {
                    break;
                }

                city = city.Trim();

                if (city.ToLower() == "вихід")
                {
                    Console.WriteLine("До побачення!");
                    break;
                }

                if (city.ToLower() == "історія")
                {
                    ShowHistory(history);
                    continue;
                }

                if (city == "")
                {
                    Console.WriteLine("Помилка: назву міста не введено.");
                    continue;
                }

                try
                {
                    string url = "https://geocoding-api.open-meteo.com/v1/search?name="
                        + Uri.EscapeDataString(city) + "&count=1&language=uk";

                    string json = await client.GetStringAsync(url);
                    GeoResponse geo = JsonSerializer.Deserialize<GeoResponse>(json, options);

                    if (geo == null || geo.Results == null || geo.Results.Count == 0)
                    {
                        Console.WriteLine("Місто не знайдено.");
                        continue;
                    }

                    GeoResult place = geo.Results[0];

                    string lat = place.Latitude.ToString(CultureInfo.InvariantCulture);
                    string lon = place.Longitude.ToString(CultureInfo.InvariantCulture);

                    string forecastUrl = "https://api.open-meteo.com/v1/forecast?latitude=" + lat
                        + "&longitude=" + lon
                        + "&daily=temperature_2m_max,temperature_2m_min,precipitation_sum"
                        + "&forecast_days=5&timezone=auto";

                    string forecastJson = await client.GetStringAsync(forecastUrl);
                    ForecastResponse forecast = JsonSerializer.Deserialize<ForecastResponse>(forecastJson, options);

                    if (forecast == null || forecast.Daily == null)
                    {
                        Console.WriteLine("Сервер не повернув дані прогнозу.");
                        continue;
                    }

                    PrintForecast(place, forecast.Daily);

                    HistoryEntry entry = new HistoryEntry();
                    entry.City = place.Name;
                    entry.Country = place.Country;
                    entry.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    history.Add(entry);

                    //остаються тільки останні 10 записів
                    while (history.Count > 10)
                    {
                        history.RemoveAt(0);
                    }

                    SaveHistory(history);
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("Помилка мережі: перевірте інтернет-з'єднання або спробуйте пізніше.");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Сервер не відповів вчасно. Спробуйте ще раз.");
                }
                catch (JsonException)
                {
                    Console.WriteLine("Не вдалося розібрати відповідь сервера.");
                }
            }
        }
    }
}