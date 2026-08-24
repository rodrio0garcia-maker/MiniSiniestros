using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSiniestros.Services;

public interface JwtSettingsBase
{
    string Key { get; }
    string Issuer { get; }
    string Audience { get; }
    int ExpiryMinutes { get; }
}
