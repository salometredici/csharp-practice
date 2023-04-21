using MediatR;
using Refit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Transactions.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Transactions.Bootstrap.Extensions;
using System.Text;
using System.Text.Json.Serialization;

namespace Transactions.API
{
    public class ApplicationStartup
    {
        public IConfiguration Configuration { get; }

        public ApplicationStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGenerator();
            services.AddRefitClient<ICurrenciesClient>()
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    client.BaseAddress = new Uri(Configuration.GetSection("CurrencyAPIUrl").Value!);
                    client.DefaultRequestHeaders.Add("apikey", Configuration.GetSection("CurrencyAPIkey").Value!);
                });
            services.AddHttpContextAccessor();

            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<ICurrenciesService, CurrenciesService>();
            services.AddScoped<ITransactionsRepository, TransactionsRepository>();

            var assembly = AppDomain.CurrentDomain.Load("Transactions.Application");
            services.AddMediatR(assembly);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("JwtPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                });
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!))
                    };
                });

            services.AddControllers()
                .AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var forwardedHeadersOptions = new ForwardedHeadersOptions() { ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers().RequireAuthorization("JwtPolicy");
            });
        }
    }
}
