// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore
{
    /// <summary>
    /// Binds Kestrel configuration.
    /// </summary>
    public class KestrelServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        private IServiceProvider _services;

        /// <summary>
        /// Creates a new instance of <see cref="KestrelServerOptionsSetup"/>.
        /// </summary>
        /// <param name="services">An <seealso cref="IServiceProvider"/> instance.</param>
        public KestrelServerOptionsSetup(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Configures a <seealso cref="KestrelServerOptions"/> instance.
        /// </summary>
        /// <param name="options">The <seealso cref="KestrelServerOptions"/> to configure.</param>
        public void Configure(KestrelServerOptions options)
        {
            options.ApplicationServices = _services;

            var configuration = _services.GetService<IConfiguration>();
            BindConfiguration(options, configuration);
        }

        private static void BindConfiguration(
            KestrelServerOptions options,
            IConfiguration configurationRoot)
        {
            var certificates = CertificateLoader.LoadAll(configurationRoot);
            var endPoints = configurationRoot.GetSection("Kestrel:EndPoints");

            foreach (var endPoint in endPoints.GetChildren())
            {
                BindEndPoint(options, configurationRoot, endPoint, certificates);
            }
        }

        private static void BindEndPoint(
            KestrelServerOptions options,
            IConfiguration configurationRoot,
            IConfigurationSection endPoint,
            Dictionary<string, X509Certificate2> certificates)
        {
            options.Listen(IPAddress.Parse(endPoint.GetValue<string>("Address")), endPoint.GetValue<int>("Port"), listenOptions =>
            {
                var certificateName = endPoint.GetValue<string>("Certificate");

                X509Certificate2 endPointCertificate = null;
                if (certificateName != null)
                {
                    endPointCertificate = certificates[certificateName];
                }
                else
                {
                    var certificate = endPoint.GetSection("Certificate");

                    if (certificate.GetChildren().Any())
                    {
                        var password = configurationRoot[$"Kestrel:EndPoints:{endPoint.Key}:Certificate:Password"];
                        endPointCertificate = CertificateLoader.Load(certificate, password);
                    }
                }

                if (endPointCertificate != null)
                {
                    listenOptions.UseHttps(endPointCertificate);
                }
            });
        }
    }
}
