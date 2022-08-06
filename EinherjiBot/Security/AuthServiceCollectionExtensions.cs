using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.Security.API.Services;
using TehGM.EinherjiBot.Security.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthBackend(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDiscordClient();
            services.AddMongoDB();
            services.AddEntityCaching();
            services.TryAddSingleton<IUserSecurityDataStore, MongoUserSecurityDataStore>();
            services.TryAddScoped<IDiscordAuthProvider, DiscordSocketAuthProvider>();
            services.TryAddScoped<IAuthProvider>(services => services.GetRequiredService<IDiscordAuthProvider>());
            services.TryAddScoped<IDiscordAuthContext>(services => services.GetRequiredService<IDiscordAuthProvider>().User);
            services.TryAddScoped<IAuthContext>(services => services.GetRequiredService<IDiscordAuthContext>());

            services.TryAddTransient<IBotAuthorizationService, BotAuthorizationService>();
            services.TryAddScoped<AuthContextMiddleware>();
            services.TryAddScoped<BotAuthorizationMiddleware>();

            services.AddHttpClient<IDiscordAuthHttpClient, DiscordAuthHttpClient>();
            services.TryAddTransient<IDiscordHttpClient>(services => services.GetRequiredService<IDiscordAuthHttpClient>());
            services.TryAddTransient<IRefreshTokenGenerator, RefreshTokenGenerator>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ => { });
            services.TryAddTransient<IAuthService, ApiAuthService>();
            services.TryAddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

            services.TryAddScoped<IUserFeatureProvider, UserFeatureProvider>();
            services.TryAddSingleton<IRefreshTokenStore, MongoRefreshTokenStore>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
            services.TryAddTransient<IJwtGenerator, JwtGenerator>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>());
            services.AddTransient<IConfigureOptions<JwtOptions>, ConfigureApiKeysOptions>();

            return services;
        }

        public class ConfigureApiKeysOptions : IConfigureOptions<JwtOptions>
        {
            public void Configure(JwtOptions options)
            {
                if (!string.IsNullOrWhiteSpace(options.PublicKeyBase64))
                {
                    byte[] bytes = Convert.FromBase64String(options.PublicKeyBase64);
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.ImportRSAPublicKey(bytes, out _);
                    options.PublicKey = new RsaSecurityKey(rsa);
                }
                if (!string.IsNullOrWhiteSpace(options.PrivateKeyBase64))
                {
                    byte[] bytes = Convert.FromBase64String(options.PrivateKeyBase64);
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.ImportRSAPrivateKey(bytes, out _);
                    options.PrivateKey = new RsaSecurityKey(rsa);
                }
            }
        }

        public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
        {
            private readonly IOptionsMonitor<JwtOptions> _jwtOptions;

            public ConfigureJwtBearerOptions(IOptionsMonitor<JwtOptions> jwtOptions)
            {
                this._jwtOptions = jwtOptions;
            }

            public void PostConfigure(string name, JwtBearerOptions options)
            {
                if (options.TokenValidationParameters == null)
                    options.TokenValidationParameters = new TokenValidationParameters();

                JwtOptions o = this._jwtOptions.CurrentValue;

                // signature
                options.TokenValidationParameters.IssuerSigningKey = o.PublicKey;
                options.TokenValidationParameters.RequireSignedTokens = true;
                options.TokenValidationParameters.ValidateIssuerSigningKey = true;

                // issuer
                options.TokenValidationParameters.ValidIssuer = o.Issuer;
                options.TokenValidationParameters.ValidateIssuer = !string.IsNullOrWhiteSpace(o.Issuer);

                // audience
                options.TokenValidationParameters.ValidAudience = o.Audience;
                options.TokenValidationParameters.ValidateAudience = !string.IsNullOrWhiteSpace(o.Audience);
                options.TokenValidationParameters.RequireAudience = !string.IsNullOrWhiteSpace(o.Audience);

                // expiration
                options.TokenValidationParameters.RequireExpirationTime = true;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);

                // claims mapping
                options.SecurityTokenValidators.OfType<JwtSecurityTokenHandler>();
                options.TokenValidationParameters.NameClaimType = ClaimNames.UserID;
                options.TokenValidationParameters.RoleClaimType = ClaimNames.Roles;

                options.SaveToken = true;
            }
        }
    }
}
