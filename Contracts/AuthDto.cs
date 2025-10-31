public record LoginRequest(string Username, string Password);
public record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt, string Username, string Role);
