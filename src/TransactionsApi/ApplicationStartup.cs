using MediatR;
using Refit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Transactions.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            services.AddControllers();
            services.AddHttpContextAccessor();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidIssuer = Configuration.GetSection("Jwt:Issuer").Value,
            //        ValidAudience = Configuration.GetSection("Jwt:Audience").Value,
            //        IssuerSigningKey = new SymmetricSecurityKey()
            //    }
            //});

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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SymmetricKey"]!))
                    };
                });

            services.AddRefitClient<ICurrenciesClient>()
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    client.BaseAddress = new Uri(Configuration.GetSection("CurrencyAPIUrl").Value!);
                    client.DefaultRequestHeaders.Add("apikey", Configuration.GetSection("CurrencyAPIkey").Value!);
                });

            services.AddScoped<ICurrenciesService, CurrenciesService>();
            services.AddScoped<ITransactionsRepository, TransactionsRepository>();

            var assembly = AppDomain.CurrentDomain.Load("Transactions.Application");
            services.AddMediatR(assembly);
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
