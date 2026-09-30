using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
            ?? "SportChain_Ultra_Secret_Key_For_Jwt_Auth_2026_Secure_Token!";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.BranchId.HasValue)
        {
            claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: "SportChainServer",
            audience: "SportChainClient",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
