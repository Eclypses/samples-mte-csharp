// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-20-2022
// ***********************************************************************
// <copyright file="Startup.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Text;
using MteSDRTest.Common.Helpers;
using MteSDRTest.Server.Helpers;
using MteSDRTest.Server.Models;
using MteSDRTest.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace MteSDRTest.Server {
    /// <summary>
    /// Class Startup.
    /// </summary>
    public class Startup {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfiguration _appConfiguration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration) {
            _appConfiguration = configuration;
        }

        #region ConfigureServices (Configures the Dependency Injection services).

        /// <summary>
        /// Configures the DI services.
        /// This method gets called by the runtime. Use this method to add services to the DI container.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services) {
            services.Configure<JwtIssuerOptions>(_appConfiguration.GetSection("JwtIssuerOptions"));

            //
            // Register helper classes
            //
            services.AddSingleton<ICryptoHelper, CryptoHelper>()
                    .AddSingleton<IJwtHelper, JwtHelper>()
                    .AddSingleton<IPayloadHelper, PayloadHelper>()
                    .AddSingleton<ICacheHelper, CacheHelper>()
                    .AddSingleton<IECDHHelper, ECDHHelper>()
                    .AddSingleton<IMteHelper, MteHelper>();

            //
            // Register application services.
            //
            services.AddSingleton<IDataExchangeService, DataExchangeService>()
                    .AddSingleton<IAuthService, AuthService>();

            //
            // Register framework classes.
            //
            // To ensure workstation local storage is persistent across sessions,
            // Store cached items in Memcached. You must include this .Net package
            // to access it.  The server name and port are stored in appsettings.
            // Nuget package: "EnyimMemcachedCore" Version="2.5.4".
            //
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "MteSDRTest.Server", Version = "v1" }); })
                    .AddEnyimMemcached(opt => {
                        opt.AddServer(_appConfiguration["AppSettings:MemcacheServer"], int.Parse(_appConfiguration["AppSettings:MemcachePort"]));
                    })
                    .AddOptions()
                    .AddControllers();

            //
            // CORS Policy - gets the allowed origins from the appsettings file
            // without this, the client will get a Fail To Fetch error.
            //
            var allowedOriginSetting = _appConfiguration["AppSettings:AllowedOrigins"];
            string[] allowedOrigins = allowedOriginSetting.Split('|');
            services.AddCors(options => {
                options.AddPolicy(
                    "CorsPolicy",
                    builder => {
                        builder.AllowAnyHeader()
                         .AllowAnyMethod()
                         .WithOrigins(allowedOrigins)
                         .AllowCredentials();
                    });
            });

            //
            // Configure the Jwt Authentication scheme
            //
            AddJwtAuthentication(services);
        }
        #endregion

        #region Configure (Configures the pipeline).

        /// <summary>
        /// Configures the specified application pipeline.
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MteSDRTest.Server v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEnyimMemcached();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
        #endregion

        #region AddJwtAuthentication

        /// <summary>
        /// Configures the Jwt Authentication scheme from values in the appsettings.json file.
        /// </summary>
        /// <param name="services">The services.</param>
        private void AddJwtAuthentication(IServiceCollection services) {
            //
            // Configure JWT authentication from the 'JwtIssuerOptions' values in the appsettings.json file
            //
            var keyBytes = Encoding.UTF8.GetBytes(_appConfiguration["JwtIssuerOptions:JwtSecret"]);
            var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);
            services.AddAuthentication(a => {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(b => {
                b.RequireHttpsMetadata = false;
                b.SaveToken = true;
                b.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
                    ValidAudience = _appConfiguration["JwtIssuerOptions:Audience"],
                    ValidIssuer = _appConfiguration["JwtIssuerOptions:Issuer"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmetricSecurityKey,
                    TokenDecryptionKey = symmetricSecurityKey,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                };
            });
        }
        #endregion

    }
}
