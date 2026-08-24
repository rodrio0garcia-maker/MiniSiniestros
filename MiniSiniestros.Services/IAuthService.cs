using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSiniestros.Services;

public interface IAuthService
{
    string? ValidarYGenerarToken(string username, string password);
}