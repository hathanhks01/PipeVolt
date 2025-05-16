using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

namespace PipeVolt_Api.Configurations
{
    public static class AuthorizationPolicySetup
    {
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Policy for Admin only (UserType = 0)
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireClaim("userType", "0"));

                // Policy for Employee only (UserType = 1)
                options.AddPolicy("RequireEmployee", policy =>
                    policy.RequireClaim("userType", "1"));

                // Policy for Customer only (UserType = 2)
                options.AddPolicy("RequireCustomer", policy =>
                    policy.RequireClaim("userType", "2"));

                // Policy for Admin and Employee (UserType = 0 or 1)
                options.AddPolicy("RequireAdminOrEmployee", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == "userType" && (c.Value == "0" || c.Value == "1"))));
            });
        }

        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                };
            });
        }
    }
}
