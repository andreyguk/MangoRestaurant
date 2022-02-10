using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Mango.Services.IdentityNew
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
            new ApiScope("Mango", "Mango Server"),               
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
            // m2m client credentials flow client
            new Client
            {
               ClientId ="client",
                    ClientSecrets = { new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes={ "read","write", "profile"}
            },

            // interactive client using code flow + pkce
            new Client
            {

                ClientId ="mango",
                    ClientSecrets = { new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:44349/signin-oidc" },
                    PostLogoutRedirectUris =  { "https://localhost:44349/signout-callback-oidc" },
                    AllowedScopes={
                         IdentityServerConstants.StandardScopes.OpenId,
                         IdentityServerConstants.StandardScopes.Profile,
                         IdentityServerConstants.StandardScopes.Email,
                         "Mango"
                     }

                //ClientId = "interactive",
                //ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                //AllowedGrantTypes = GrantTypes.Code,

                //RedirectUris = { "https://localhost:44300/signin-oidc" },
                //FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                //PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                //AllowOfflineAccess = true,
                //AllowedScopes = { "openid", "profile", "scope2" }
            },
            };
    }
}