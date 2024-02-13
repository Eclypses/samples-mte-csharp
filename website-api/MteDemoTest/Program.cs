using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MteDemoTest.Models;
using Serilog;

namespace MteDemoTest
{
    public class Program
    {
        public static IConfiguration _configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("AppName", System.Reflection.Assembly.GetEntryAssembly().GetName().Name)
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .CreateLogger();
            try
            {
                Log.Information("****************************************************************************************************************************");
                Log.Information("\t\t\t\t\t\tStarting {0} on {1} using {2} AppSettings", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, Environment.MachineName, Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                Log.Information("****************************************************************************************************************************");

                //-------------------------------------------
                // Create session IV and save to memory cache
                //-------------------------------------------
                string encIV = Guid.NewGuid().ToString();
                Constants.MteClientState.Store(Constants.IVKey, encIV, TimeSpan.FromMinutes(Constants.IVExpirationMinutes));

                CreateHostBuilder(args).Build().Run();

                
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
