using Microsoft.AspNetCore.Identity;

namespace MotoRevApi.Model;

public class Usuario : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
