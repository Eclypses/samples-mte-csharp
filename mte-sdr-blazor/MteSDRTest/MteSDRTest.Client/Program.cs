// ***********************************************************************
// Assembly         : MteSDRTest.Client
// Author           : Eclypses Developer
// Created          : 07-19-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 08-08-2022
// ***********************************************************************
// <copyright file="Program.cs" company="MteSDRTest.Client">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MteSDRTest.Client.Helpers;
using MteSDRTest.Client.Models;
using MteSDRTest.Client.Services;
using MteSDRTest.Common.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MteSDRTest.Client {
    /// <summary>
    /// Class Program.
    /// </summary>
    public class Program {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>Completed task.</returns>
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            //
            // Get the url of the API server from wwwroot/appsettings.json
            //
            string apiServerUrl = builder.Configuration.GetValue<string>("AppSettings:APIServerUrl");

            builder.Services

                //
                // Register some framework services
                //
                .AddOptions()
                .AddAuthorizationCore()
                .AddScoped<AuthenticationStateProvider, MyAuthenticationStateProvider>() // Manages the UserPrincipal of the logged in user

                //
                // Registers the  JavaScript wrappers.
                //
                .AddScoped<IECDHService, ECDHService>() // Wraps the Elliptical Curve Diffie-Hellman JavaScript calls.
                .AddScoped<IMteService, MteService>() // Wraps the Eclypses MTE - WASM JavaScript calls.
                .AddScoped<ICryptoHelper, CryptoHelper>() // Provides various cryptography services.
                .AddScoped<IBrowserStorageHelper, BrowserStorageHelper>() // Allows raw access to browser storage.

                //
                // Registers the application .Net services that do not use JavaScript.
                //
                .AddScoped<StateContainer>() // Wrapper around some local "state" values.
                .AddScoped<IPayloadService, PayloadService>() // Conceals and Reveals typed objects using MKE.
                .AddScoped<ISDRService, SDRService>() // Wraps calls to the local SDR (an MTE Add-on).
                .AddScoped<IAuthService, AuthService>() // Handles server calls to authorize the id / password.
                .AddScoped<IAlertService, AlertService>() // Manages alert messages

                //
                // Registers services required for interacting with the API server.
                //
                .AddScoped<IWebHelper, WebHelper>() // Wraps POST and GET calls to the API server.
                .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }) // Add the base address for instancing up this app.
                .AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiServerUrl) }); // The actual api server client.

            await builder.Build().RunAsync();
        }
    }
}
