using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace StreamVault.Application.Services;

public interface IPlaybackTokenService
{
    string GenerateVideoPlaybackToken(Guid videoId, Guid tenantId, TimeSpan lifetime);
    bool TryValidateVideoPlaybackToken(string token, Guid expectedVideoId, out Guid tenantId);
}

public class PlaybackTokenService : IPlaybackTokenService
{
    private readonly string _secretKey;

    public PlaybackTokenService(IConfiguration configuration)
    {
        _secretKey =
            configuration["JwtSettings:SecretKey"]
            ?? configuration["JwtSettings:SigningKey"]
            ?? configuration["Jwt:SecretKey"]
            ?? configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "JWT signing key not configured (JwtSettings:SecretKey/JwtSettings:SigningKey or Jwt:SecretKey/Jwt:SigningKey)");
    }

    public string GenerateVideoPlaybackToken(Guid videoId, Guid tenantId, TimeSpan lifetime)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new("video_id", videoId.ToString()),
            new("tenant_id", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool TryValidateVideoPlaybackToken(string token, Guid expectedVideoId, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);

            var videoClaim = principal.FindFirst("video_id")?.Value;
            var tenantClaim = principal.FindFirst("tenant_id")?.Value;

            if (!Guid.TryParse(videoClaim, out var videoId) || videoId != expectedVideoId)
                return false;

            if (!Guid.TryParse(tenantClaim, out tenantId))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
