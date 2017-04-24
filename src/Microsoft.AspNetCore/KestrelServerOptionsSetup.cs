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
        private readonly IConfiguration _configurationRoot;

        /// <summary>
        /// Creates a new instance of <see cref="KestrelServerOptionsSetup"/>.
        /// </summary>
        /// <param name="configurationRoot">The root <seealso cref="IConfiguration"/>.</param>
        public KestrelServerOptionsSetup(IConfiguration configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }

        /// <summary>
        /// Configures a <seealso cref="KestrelServerOptions"/> instance.
        /// </summary>
        /// <param name="options">The <seealso cref="KestrelServerOptions"/> to configure.</param>
        public void Configure(KestrelServerOptions options)
        {
            BindConfiguration(options);
        }

        private void BindConfiguration(KestrelServerOptions options)
        {
            var certificates = CertificateLoader.LoadAll(_configurationRoot);
            var endPoints = _configurationRoot.GetSection("Kestrel:EndPoints");

            foreach (var endPoint in endPoints.GetChildren())
            {
                BindEndPoint(options, endPoint, certificates);
            }
        }

        private void BindEndPoint(
            KestrelServerOptions options,
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
                        var password = _configurationRoot[$"Kestrel:EndPoints:{endPoint.Key}:Certificate:Password"];
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
