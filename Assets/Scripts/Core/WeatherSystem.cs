using UnityEngine;

/// <summary>
/// Menghitung hasil roll cuaca harian (60% Cerah, 25% Berawan, 15% Hujan Badai — gaya Persona).
/// Diekstraksi dari GameManager.RollDailyWeather(): GameManager tetap memutuskan KAPAN cuaca
/// di-roll dan APA yang terjadi setelahnya (set flag, update HUD); kelas ini hanya menjawab
/// "bagaimana hasil roll dihitung".
/// </summary>
public static class WeatherSystem
{
    public static WeatherState RollDailyWeather()
    {
        float roll = UnityEngine.Random.value;
        if (roll < 0.60f) return WeatherState.Cerah;
        if (roll < 0.85f) return WeatherState.Berawan;
        return WeatherState.HujanBadai;
    }
}
