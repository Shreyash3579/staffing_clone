using System.IdentityModel.Tokens.Jwt;

namespace Staffing.Authentication.Tests.Helpers
{
    public static class JwtTokenHelper
    {
        public static JwtSecurityToken GetDecodedJwtToken(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonTOken = handler.ReadToken(jwtToken);
            var decodedToken = jsonTOken as JwtSecurityToken;
            return decodedToken;
        }
    }
}
