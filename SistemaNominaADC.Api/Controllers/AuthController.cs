using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            ModelState.AddModelError(nameof(LoginRequestDTO.Email), "Email es obligatorio.");
            ModelState.AddModelError(nameof(LoginRequestDTO.Password), "Password es obligatorio.");
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(Problem(statusCode: StatusCodes.Status401Unauthorized, title: "No autorizado", detail: "Credenciales inválidas"));

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(GenerarToken(user, roles.ToList()));
    }

    private LoginResponseDTO GenerarToken(ApplicationUser user, List<string> roles)
    {
        var jwt = _configuration.GetSection("Jwt");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new LoginResponseDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            UserName = user.UserName ?? user.Email ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }
}
