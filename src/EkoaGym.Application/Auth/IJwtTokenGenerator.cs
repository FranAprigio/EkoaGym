using EkoaGym.Domain.Entities;

namespace EkoaGym.Application.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
}
