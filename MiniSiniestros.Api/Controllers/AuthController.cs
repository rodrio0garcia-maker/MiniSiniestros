using Microsoft.AspNetCore.Mvc;
using MiniSiniestros.Dto;
using MiniSiniestros.Services;

namespace MiniSiniestros.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto dto)
    {
        var token = _authService.ValidarYGenerarToken(dto.Username, dto.Password);
        if (token is null)
            return Unauthorized();

        return Ok(new { token });
    }
}
