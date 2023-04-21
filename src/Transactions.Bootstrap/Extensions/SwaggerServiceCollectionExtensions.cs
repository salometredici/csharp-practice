using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace Transactions.Bootstrap.Extensions
{
    public static class SwaggerServiceCollectionExtensions
    {
        public const string Bearer = "Bearer";

        public static void AddSwaggerGenerator(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition(Bearer, new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header. Ex.: 'Bearer ' + token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Name = HeaderNames.Authorization,
                    Scheme = Bearer
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = Bearer,
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}
