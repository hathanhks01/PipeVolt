using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PipeVolt_DAL.Common; 
using System.Text;
using static PipeVolt_DAL.Common.DataType;

namespace PipeVolt_Api.Configurations
{
    public static class AuthorizationPolicySetup
    {
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Policy for Admin only
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireClaim("userType", ((int)UserType.Admin).ToString()));

                // Policy for Employee only
                options.AddPolicy("RequireEmployee", policy =>
                    policy.RequireClaim("userType", ((int)UserType.Employee).ToString()));

                // Policy for Customer only
                options.AddPolicy("RequireCustomer", policy =>
                    policy.RequireClaim("userType", ((int)UserType.Customer).ToString()));

                // Policy for Admin and Employee
                options.AddPolicy("RequireAdminOrEmployee", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == "userType" &&
                            (c.Value == ((int)UserType.Admin).ToString() ||
                             c.Value == ((int)UserType.Employee).ToString()))));
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
        public static void AddGoogleAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = configuration["Google:ClientId"];
                    options.ClientSecret = configuration["Google:ClientSecret"];
                    options.CallbackPath = "/signin-google";
                    options.SignInScheme = JwtBearerDefaults.AuthenticationScheme;
                });
        }
    }
}