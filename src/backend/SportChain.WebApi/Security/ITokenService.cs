using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Security;

public interface ITokenService
{
    string GenerateJwtToken(User user);
}
