// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-21-2022
// ***********************************************************************
// <copyright file="Program.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MteSDRTest.Server {
    /// <summary>
    /// Class Program.
    /// </summary>
    public class Program {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public static IConfiguration _configuration { get; } = new ConfigurationBuilder()
#pragma warning restore SA1300 // Element should begin with upper-case letter
              .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local"}.json", optional: true)
                  .AddEnvironmentVariables()
                  .Build();

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args) {
            //
            // Set up the Serilog logger - it uses the Microsoft ILogger interface and is injectable anywhere
            //
            var appName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("AppName", appName)
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .CreateLogger();
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>IHostBuilder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
