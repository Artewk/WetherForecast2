# Автоматизоване тестування

Практична робота №16. Автоматизоване тестування

Проєкт: консольна програма прогнозу погоди. Група ІПЗ-11, ЛНТУ

## 1. Які функції доцільно тестувати автоматично

| Функція | Тестуємо автоматично? | Чому |
| --- | --- | --- |
| Розбір JSON відповіді геокодингу | Так | Чиста функція: той самий вхід завжди дає той самий результат |
| Розбір JSON відповіді з прогнозом | Так | Те саме, і саме тут найлегше зламати зв’язок між JSON і класами |
| Збереження й читання історії (JSON) | Так | Не залежить від мережі, перевіряється за мілісекунди |
| Запити до Open-Meteo | Ні | Залежать від мережі й зовнішнього сервісу, результат нестабільний |
| Вивід у консоль | Ні | Складно перевіряти автоматично, перевірено вручну (практична №15) |

## 2. Тестовий проєкт

Для тестів використано бібліотеку **xUnit**. Налаштування:

1. До рішення додано проєкт **xUnit Test Project** з назвою `WetherForecast2.Tests` (.NET 9).
2. У тестовому проєкті додано посилання на основний проєкт `WetherForecast2` (Dependencies → Add Project Reference).
3. Класи в папці `Models` зроблено `public`, щоб тестовий проєкт міг їх використовувати.

## 3. Реалізовані тести

| ID | Назва тесту | Тип | Що перевіряє | Очікуваний результат |
| --- | --- | --- | --- | --- |
| AT-1 | Forecast_ParsesDailyValues | Позитивний | Розбір JSON прогнозу на два дні | Кількість днів, дата, температури й опади збігаються з вхідними даними |
| AT-2 | Geo_ParsesCity | Позитивний | Розбір JSON знайденого міста | Список містить одне місто з правильними назвою й широтою |
| AT-3 | Geo_CityNotFound_ResultsIsNull | Негативний | Відповідь без ключа results (місто не знайдено) | Властивість Results дорівнює null |
| AT-4 | InvalidJson_ThrowsJsonException | Негативний | Некоректний текст замість JSON | Виникає виняток JsonException |
| AT-5 | History_SerializeAndDeserialize_KeepsData | Позитивний | Перетворення історії на JSON і назад | Дані запису (місто, дата) не змінюються |

Код тестів (файл `JsonParsingTests.cs`):

```csharp
using System.Collections.Generic;
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
```

## 4. Запуск тестів

1. У меню Visual Studio відкрити **Test → Run All Tests** (або натиснути Ctrl+R, потім A).
2. Відкриється вікно **Test Explorer** (якщо його не видно: **Test → Test Explorer**). Воно показує список тестів і результат кожного.
3. Окремий тест можна запустити, клікнувши на ньому правою кнопкою й вибравши **Run**. Якщо тест треба проаналізувати детально, є пункт **Debug**: виконання зупиниться на точках зупину.

Тести виконуються без інтернету й за мілісекунди, тому їх зручно запускати після кожної зміни коду.

## 5. Результати запуску

| Показник | Значення |
| --- | --- |
| Дата запуску | [дата] |
| Усього тестів | 5 |
| Пройдено | [кількість] |
| Не пройдено | [кількість] |

[Вставити скріншот вікна Test Explorer]

## 6. Аналіз результатів

### 6.1. Як читати Test Explorer

- **Зелена галочка** — тест пройдено: фактичний результат збігся з очікуваним.
- **Червоний хрестик** — тест не пройдено: результат інший або виникла помилка.
- **Синя позначка** — тест ще не запускався після останньої зміни коду.
- У нижній частині вікна для обраного тесту показано **повідомлення про помилку**, тривалість виконання й рядок коду, де сталася розбіжність.

### 6.2. Що робити, якщо тест не пройдено

1. Прочитати повідомлення. Для `Assert.Equal` воно містить рядки Expected (очікувалося) та Actual (отримано).
2. Відкрити рядок, на який вказує повідомлення, і зрозуміти, що саме не збіглося.
3. Вирішити, де помилка: у програмі (тоді виправити код) чи в самому тесті (тоді виправити очікуване значення).
4. Запустити тести знову й переконатися, що все зелене.

### 6.3. Приклади того, як тести виявляють помилки

| Експеримент | Що відбувається | Висновок |
| --- | --- | --- |
| У тесті AT-1 замінити очікуване значення 12.1 на 99 | Тест червоний: повідомлення Assert.Equal() Failure, Expected: 99, Actual: 12.1 | Тест порівнює результат із очікуваним і виявляє розбіжність |
| З класу DailyData прибрати атрибут [JsonPropertyName("temperature_2m_max")] | Тест AT-1 падає: властивість TempMax не заповнилась (null) | Тест виявляє зламаний зв’язок між назвою поля в JSON і властивістю класу |

Після експерименту зміни потрібно повернути.

### 6.4. Виявлені помилки

[Записати, чи виявили тести помилки. Якщо всі тести пройшли, написати: «Помилок не виявлено».]

## 7. Висновки

Автоматизовані тести перевіряють найкрихкіші місця програми: розбір відповідей сервісу й збереження історії. Вони не залежать від мережі, виконуються миттєво й одразу показують, що саме зламалось після зміни коду. Позитивні тести підтверджують, що правильні дані обробляються коректно, а негативні (місто не знайдено, некоректний JSON) — що програма поводиться передбачувано на неправильних даних. Запити до сервісу й консольний вивід перевірено вручну за тест-кейсами практичної №15.
