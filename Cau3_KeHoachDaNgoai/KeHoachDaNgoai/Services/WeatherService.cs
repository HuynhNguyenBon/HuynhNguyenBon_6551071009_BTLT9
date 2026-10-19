using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace KeHoachDaNgoai.Services;

public sealed record ForecastResult(int WeatherCode, double MinC, double MaxC, int RainProbability, string Place);

public sealed class WeatherService(HttpClient http)
{
    public async Task<ForecastResult?> GetForecastAsync(string place, DateTime travelDate, CancellationToken token = default)
    {
        if ((travelDate.Date - DateTime.Today).TotalDays > 16 || travelDate.Date < DateTime.Today) return null;
        var geoUrl = "https://geocoding-api.open-meteo.com/v1/search?name=" + Uri.EscapeDataString(place)
            + "&count=1&language=vi&format=json";
        using var geoDoc = await http.GetFromJsonAsync<JsonDocument>(geoUrl, token);
        var result = geoDoc?.RootElement.GetProperty("results");
        if (result is null || result.Value.GetArrayLength() == 0) throw new KeyNotFoundException("Không tìm thấy địa điểm.");
        var location = result.Value[0];
        var lat = location.GetProperty("latitude").GetDouble();
        var lon = location.GetProperty("longitude").GetDouble();
        var matchedName = location.TryGetProperty("name", out var n) ? n.GetString() ?? place : place;
        var url = string.Create(CultureInfo.InvariantCulture,
            $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max&timezone=auto&forecast_days=16");
        using var forecast = await http.GetFromJsonAsync<JsonDocument>(url, token);
        if (forecast is null) throw new HttpRequestException("API thời tiết trả về dữ liệu rỗng.");
        var daily = forecast.RootElement.GetProperty("daily");
        var dates = daily.GetProperty("time");
        var key = travelDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var index = -1;
        for (var i = 0; i < dates.GetArrayLength(); i++) if (dates[i].GetString() == key) { index = i; break; }
        if (index < 0) return null;
        return new ForecastResult(
            daily.GetProperty("weather_code")[index].GetInt32(),
            daily.GetProperty("temperature_2m_min")[index].GetDouble(),
            daily.GetProperty("temperature_2m_max")[index].GetDouble(),
            daily.GetProperty("precipitation_probability_max")[index].GetInt32(),
            matchedName);
    }
}
