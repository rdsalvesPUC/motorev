namespace MotoRevApi.Dto.Response;

public record LoginResponse(
    string Token,
    string RefreshToken,
    string Perfil,
    UserData Usuario
);

public record UserData(
    int Id,
    string Nome
);