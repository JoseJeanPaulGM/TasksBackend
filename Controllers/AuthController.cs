using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infra;
using Services;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    public AuthController(AppDbContext db, JwtService jwt) { _db = db; _jwt = jwt; }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new ErrorResponse(401, "Unauthorized", "Credenciales inválidas."));

        var (token, exp) = _jwt.CreateToken(user);
        return Ok(new LoginResponse(token, "Bearer", exp, user.Username, user.Role));
    }
}
