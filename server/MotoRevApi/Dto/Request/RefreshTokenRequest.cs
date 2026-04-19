namespace MotoRevApi.Dto.Request;

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
