using MiniSiniestros.Services;

namespace MiniSiniestros.Api.Configuration;

public class JwtSettings : JwtSettingsBase
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}
