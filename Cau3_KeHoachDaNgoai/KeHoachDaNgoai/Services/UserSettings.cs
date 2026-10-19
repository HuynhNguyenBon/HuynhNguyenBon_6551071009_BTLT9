using System.Text.Json;

namespace KeHoachDaNgoai.Services;

public sealed class UserSettings
{
    public string Language { get; set; } = "vi";
    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KeHoachDaNgoai", "usersettings.json");

    public static UserSettings Load()
    {
        try { return File.Exists(SettingsPath) ? JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(SettingsPath)) ?? new() : new(); }
        catch { return new(); }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
