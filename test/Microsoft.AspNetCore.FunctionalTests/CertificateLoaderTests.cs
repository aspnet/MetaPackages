// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Microsoft.AspNetCore.FunctionalTests
{
    public class CertificateLoaderTests : IDisposable
    {
        private readonly IConfiguration _configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Certificates:Certificate1:Source"] = "File",
                ["Certificates:Certificate1:Path"] = "TestArtifacts/Certificate1.pfx",
                ["Certificates:Certificate1:Password"] = "Password1",
                ["Certificates:Certificate2:Source"] = "File",
                ["Certificates:Certificate2:Path"] = "TestArtifacts/Certificate2.pfx",
                ["Certificates:Certificate2:Password"] = "Password2",
                ["TestConfig:CertificateName"] = "Certificate1",
                ["TestConfig:CertificateNames"] = "Certificate1 Certificate2",
                ["TestConfig:Certificate:Source"] = "File",
                ["TestConfig:Certificate:Path"] = "TestArtifacts/Certificate3.pfx",
                ["TestConfig:Certificate:Password"] = "Password3",
                ["TestConfig:Certificates:Certificate4:Source"] = "File",
                ["TestConfig:Certificates:Certificate4:Path"] = "TestArtifacts/Certificate4.pfx",
                ["TestConfig:Certificates:Certificate4:Password"] = "Password4",
                ["TestConfig:Certificates:Certificate5:Source"] = "File",
                ["TestConfig:Certificates:Certificate5:Path"] = "TestArtifacts/Certificate5.pfx",
                ["TestConfig:Certificates:Certificate5:Password"] = "Password5",
            })
            .Build();

        private readonly X509Certificate2 _certificate1 = new X509Certificate2("TestArtifacts/Certificate1.pfx", "Password1");
        private readonly X509Certificate2 _certificate2 = new X509Certificate2("TestArtifacts/Certificate2.pfx", "Password2");
        private readonly X509Certificate2 _certificate3 = new X509Certificate2("TestArtifacts/Certificate3.pfx", "Password3");
        private readonly X509Certificate2 _certificate4 = new X509Certificate2("TestArtifacts/Certificate4.pfx", "Password4");
        private readonly X509Certificate2 _certificate5 = new X509Certificate2("TestArtifacts/Certificate5.pfx", "Password5");

        public void Dispose()
        {
            _certificate1.Dispose();
            _certificate2.Dispose();
            _certificate3.Dispose();
            _certificate4.Dispose();
            _certificate5.Dispose();
        }

        [Fact]
        public void LoadsCertificateByName()
        {
            var certificate = new CertificateLoader(_configurationRoot.GetSection("Certificates"))
                .Load("Certificate1");
            Assert.Equal(_certificate1, certificate);
        }

        [Fact]
        public void LoadsCertificateByNameFromConfigurationSection()
        {
            var certificate = new CertificateLoader(_configurationRoot.GetSection("Certificates"))
                .Load(_configurationRoot.GetSection("TestConfig:CertificateName"))
                .FirstOrDefault();
            Assert.Equal(_certificate1, certificate);
        }

        [Fact]
        public void LoadsCertificatesByNameFromConfigurationSection()
        {
            var certificates = new CertificateLoader(_configurationRoot.GetSection("Certificates"))
                .Load(_configurationRoot.GetSection("TestConfig:CertificateNames"))
                .ToArray();
            Assert.Equal(_certificate1, certificates[0]);
            Assert.Equal(_certificate2, certificates[1]);
        }

        [Fact]
        public void LoadsCertificateInline()
        {
            var certificate = new CertificateLoader(_configurationRoot.GetSection("Certificates"))
                .Load(_configurationRoot.GetSection("TestConfig:Certificate"))
                .FirstOrDefault();
            Assert.Equal(_certificate3, certificate);
        }

        [Fact]
        public void LoadsMultipleCertificatesInline()
        {
            var certificates = new CertificateLoader(_configurationRoot.GetSection("Certificates"))
                .Load(_configurationRoot.GetSection("TestConfig:Certificates"))
                .ToArray();
            Assert.Equal(_certificate4, certificates[0]);
            Assert.Equal(_certificate5, certificates[1]);
        }

        [Fact]
        public void LoadsCertificateInlineWithoutCertificateReferences()
        {
            var certificate = new CertificateLoader()
                .Load(_configurationRoot.GetSection("TestConfig:Certificate"))
                .FirstOrDefault();
            Assert.Equal(_certificate3, certificate);
        }

        [Fact]
        public void LoadsMultipleCertificatesInlineWithoutCertificateReferences()
        {
            var certificates = new CertificateLoader()
                .Load(_configurationRoot.GetSection("TestConfig:Certificates"))
                .ToArray();
            Assert.Equal(_certificate4, certificates[0]);
            Assert.Equal(_certificate5, certificates[1]);
        }
    }
}
