using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace QuanLyNhaTro.Services;

public class ProvinceDto
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public sealed class ProvinceDetailDto : ProvinceDto
{
    [JsonPropertyName("districts")] public List<DistrictDto> Districts { get; set; } = [];
}

public sealed class DistrictDto
{
    [JsonPropertyName("wards")] public List<WardDto> Wards { get; set; } = [];
}

public sealed class WardDto
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public sealed class ProvinceApiClient(HttpClient http)
{
    public Task<List<ProvinceDto>?> GetProvincesAsync(CancellationToken token = default) =>
        http.GetFromJsonAsync<List<ProvinceDto>>("api/v2/", token);

    public async Task<List<WardDto>> GetWardsAsync(int provinceCode, CancellationToken token = default)
    {
        var province = await http.GetFromJsonAsync<ProvinceDetailDto>(
            $"api/v2/p/{provinceCode}?depth=2", token);
        return province?.Districts.SelectMany(d => d.Wards).ToList() ?? [];
    }
}
