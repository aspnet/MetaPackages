﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SampleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HelloWorld();

            CustomUrl();

            CustomRouter();

            CustomApplicationBuilder();

            StartupClass(args);
        }

        private static void HelloWorld()
        {
            using (var host = WebHost.Start(context => context.Response.WriteAsync("Hello, World!")))
            {
                host.WaitForShutdown();
                Console.WriteLine("Running HelloWorld: Press any key to shutdown and start the next sample...");
                Console.ReadKey();
            }
        }

        private static void CustomUrl()
        {
            // Changing the listening URL
            using (var host = WebHost.Start("http://localhost:8080", context => context.Response.WriteAsync("Hello, World!")))
            {
                host.WaitForShutdown();
                Console.WriteLine("Running CustomUrl: Press any key to shutdown and start the next sample...");
                Console.ReadKey();
            }
        }

        private static void CustomRouter()
        {
            // Using a router
            using (var host = WebHost.Start(router => router
                .MapGet("hello/{name}", (req, res, data) => res.WriteAsync($"Hello, {data.Values["name"]}"))
                .MapGet("goodbye/{name}", (req, res, data) => res.WriteAsync($"Goodbye, {data.Values["name"]}"))
                .MapGet("throw/{message?}", (req, res, data) => throw new Exception((string)data.Values["message"] ?? "Uh oh!"))
                .MapGet("{greeting}/{name}", (req, res, data) => res.WriteAsync($"{data.Values["greeting"]}, {data.Values["name"]}"))
                .MapGet("", (req, res, data) => res.WriteAsync($"Hello, World!"))))
            {
                host.WaitForShutdown();
                Console.WriteLine("Running CustomRouter: Press any key to shutdown and start the next sample...");
                Console.ReadKey();
            }
        }

        private static void CustomApplicationBuilder()
        {
            // Using a application builder
            using (var host = WebHost.StartWith(app =>
            {
                app.UseStaticFiles();
                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Hello, World!");
                });
            }))
            {
                host.WaitForShutdown();
                Console.WriteLine("Running CustomApplicationBuilder: Press any key to shutdown and start the next sample...");
                Console.ReadKey();
            }
        }

        private static void StartupClass(string[] args)
        {
            // Using defaults with a Startup class
            using (var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build())
            {
                host.Run();
            }
        }
    }
}
