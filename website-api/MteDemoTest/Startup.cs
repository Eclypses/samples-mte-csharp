using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MteDemoTest.Helpers;
using MteDemoTest.Repository;
using Serilog;
using System.Text;

namespace MteDemoTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(o => {
                //
                // This input formatter allows us to accept plain text in the controller
                o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter());
            });
            services.AddOptions();
            services.Configure<Models.AppSettings>(Configuration.GetSection("appSettings"));
            services.Configure<Models.JwtIssuerOptions>(Configuration.GetSection("JwtIssuerOptions"));
            services.AddLogging(loggingBuilder => {
                loggingBuilder.AddSerilog();
                loggingBuilder.AddDebug();
            });

            //
            // Configure JWT authentication from the 'jwtIssuerOptions' values in the appsettings.json file
            //
            Models.JwtIssuerOptions jwtSettings = Configuration.GetSection("JwtIssuerOptions").Get<Models.JwtIssuerOptions>();
            var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.JwtSecret);
            SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);

            services.AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(b =>
             {
                 b.RequireHttpsMetadata = false;
                 b.SaveToken = true;
                 b.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                 {
                     ValidAudience = jwtSettings.Audience,
                     ValidIssuer = jwtSettings.Issuer,
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = symmetricSecurityKey,
                     TokenDecryptionKey = symmetricSecurityKey,
                     ValidateIssuer = true,
                     ValidateAudience = true
                 };
             });

            // Add Helpers and Repositories
            services.AddSingleton<IFileUploadRepository, FileUploadRepository>();
            services.AddSingleton<IMultipleClientRepository, MultipleClientRepository>();
            services.AddSingleton<IMteStateHelper, MteStateHelper>();
            services.AddSingleton<ILoginRepository, LoginRepository>();
            services.AddSingleton<IAuthHelper, AuthHelper>();
            services.AddSingleton<IHandshakeRepository, HandshakeRepository>();
            services.AddDistributedMemoryCache();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MteDemoTest", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MteDemoTest"));
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
