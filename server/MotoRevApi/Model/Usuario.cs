using Microsoft.AspNetCore.Identity;

namespace MotoRevApi.Model;

public class Usuario : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Propriedade de navegação para o relacionamento 1:1
    public virtual Concessionaria? Concessionaria { get; set; }
    public virtual Cliente? Cliente { get; set; }
}
