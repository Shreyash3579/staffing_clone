using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace Microservices.Common.Core.Helpers
{
    public static class JWTHelper
    {
        public static IEnumerable<int> GetAccessibleOffices(HttpContext context)
        {
            var userClaims = context?.User?.Claims?.ToList();
            if (userClaims != null && userClaims.Any())
            {
                //retun offices user has access to in BOSS or "0" if no access in BOSS app
                //return empty for access to all offices for API's
                if (userClaims.FirstOrDefault(x => x.Type == "OfficeAccess") != null)
                {
                   var accessibleOffices = JsonConvert.DeserializeObject<IEnumerable<int>>(userClaims.FirstOrDefault(x => x.Type == "OfficeAccess").Value);

                    return accessibleOffices ?? null;
                }
                else
                {
                    return Enumerable.Empty<int>();
                }
            }
            return Enumerable.Empty<int>();
        }

        public static IEnumerable<SecurityKey> GetIssuerSigningKeyResolverForMinSecretKeySize(string tokenString, SecurityToken securityToken, string identifier, TokenValidationParameters parameters)
        {
            string alg = null;

            if (securityToken is JwtSecurityToken jwtSecurityToken)
                alg = jwtSecurityToken.SignatureAlgorithm;

            if (securityToken is Microsoft.IdentityModel.JsonWebTokens.JsonWebToken jsonWebToken)
                alg = jsonWebToken.Alg;

            if (parameters.IssuerSigningKeys.All(x => x is SymmetricSecurityKey) && alg != null)
            {
                var data = new List<SecurityKey>();

                foreach (SymmetricSecurityKey symIssKey in parameters.IssuerSigningKeys)
                {
                    // Workaround for breaking change in "System.IdentityModel.Tokens.Jwt 6.30.1+"
                    switch (alg?.ToLowerInvariant())
                    {
                        case "hs256":
                            data.Add(ExtendKeyLengthIfNeeded(symIssKey, 32));
                            break;
                    }
                }

                return data;
            }

            return new List<SecurityKey>
            {
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("staffing_secretKey"))),
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("staffing_secretKey_old")))
            };
        }

        private static SecurityKey ExtendKeyLengthIfNeeded(SymmetricSecurityKey key, int requiredLength)
        {
            if (key.KeySize < requiredLength * 8)
            {
                var paddedKey = new byte[requiredLength];
                Buffer.BlockCopy(key.Key, 0, paddedKey, 0, key.Key.Length);
                return new SymmetricSecurityKey(paddedKey);
            }
            return key;
        }
    }
}
