﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.Tests
{
    public class WebHostFunctionalTests : LoggedTest
    {
        private readonly string _testSitesPath;

        public WebHostFunctionalTests(ITestOutputHelper output) : base(output)
        {
            _testSitesPath = GetTestSitesPath();
        }

        [Fact]
        public async Task Start_RequestDelegate_Url()
        {
            await ExecuteStartOrStartWithTest(deploymentResult => deploymentResult.HttpClient.GetAsync(string.Empty), "StartRequestDelegateUrlApp");
        }

        [Fact]
        public async Task Start_RouteBuilder_Url()
        {
            await ExecuteStartOrStartWithTest(deploymentResult => deploymentResult.HttpClient.GetAsync("/route"), "StartRouteBuilderUrlApp");
        }

        [Fact]
        public async Task StartWith_IApplicationBuilder_Url()
        {
            await ExecuteStartOrStartWithTest(deploymentResult => deploymentResult.HttpClient.GetAsync(string.Empty), "StartWithIApplicationBuilderUrlApp");
        }

        [Fact]
        public async Task CreateDefaultBuilder_InitializeWithDefaults()
        {
            var applicationName = "CreateDefaultBuilderApp";
            await ExecuteTestApp(applicationName, async (deploymentResult, logger) =>
            {
                var response = await RetryHelper.RetryRequest(() => deploymentResult.HttpClient.GetAsync(string.Empty), logger, deploymentResult.HostShutdownToken);
                var errorResponse = await RetryHelper.RetryRequest(() => deploymentResult.HttpClient.GetAsync("/error"), logger, deploymentResult.HostShutdownToken);

                var responseText = await response.Content.ReadAsStringAsync();
                var errorResponseText = await errorResponse.Content.ReadAsStringAsync();
                try
                {
                    // Assert server is Kestrel
                    Assert.Equal("Kestrel", response.Headers.Server.ToString());

                    // The application name will be sent in response when all asserts succeed in the test app.
                    Assert.Equal(applicationName, responseText);

                    // Assert UseDeveloperExceptionPage is called in WebHostStartupFilter.
                    Assert.Contains("An unhandled exception occurred while processing the request.", errorResponseText);
                }
                catch (XunitException)
                {
                    logger.LogWarning(response.ToString());
                    logger.LogWarning(responseText);
                    throw;
                }
            }, setTestEnvVars: true);
        }

        [Theory]
        [InlineData("127.0.0.1", "127.0.0.1")]
        [InlineData("::1", "[::1]")]
        public async Task BindsKestrelHttpEndPointFromConfiguration(string endPointAddress, string requestAddress)
        {
            try
            {
                File.WriteAllText("appsettings.json", @"
{
    ""Kestrel"": {
        ""EndPoints"": {
            ""EndPoint"": {
                ""Address"": """ + endPointAddress + @""",
                ""Port"": 0
            }
        }
    }
}
");
                using (var webHost = WebHost.Start(context => context.Response.WriteAsync("Hello, World!")))
                {
                    var port = GetWebHostPort(webHost);

                    Assert.NotEqual(0, port);

                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync($"http://{requestAddress}:{port}");
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            finally
            {
                File.Delete("appsettings.json");
            }
        }

        [Fact]
        public async Task BindsKestrelHttpsEndPointFromConfiguration_ReferencedCertificateFile()
        {
            try
            {
                File.WriteAllText("appsettings.json", @"
{
    ""Kestrel"": {
        ""EndPoints"": {
            ""EndPoint"": {
                ""Address"": ""127.0.0.1"",
                ""Port"": 0,
                ""Certificate"": ""TestCert""
            }
        }
    },
    ""Certificates"": {
        ""TestCert"": {
            ""Source"": ""File"",
            ""Path"": ""testCert.pfx"",
            ""Password"": ""testPassword""
        }
    }
}
");
                using (var webHost = WebHost.Start(context => context.Response.WriteAsync("Hello, World!")))
                {
                    var port = GetWebHostPort(webHost);

                    var response = await HttpClientSlim.GetStringAsync($"https://127.0.0.1:{port}", validateCertificate: false);
                    Assert.Equal("Hello, World!", response);
                }
            }
            finally
            {
                File.Delete("appsettings.json");
            }
        }

        [Fact]
        public async Task BindsKestrelHttpsEndPointFromConfiguration_InlineCertificateFile()
        {
            try
            {
                File.WriteAllText("appsettings.json", @"
{
    ""Kestrel"": {
        ""EndPoints"": {
            ""EndPoint"": {
                ""Address"": ""127.0.0.1"",
                ""Port"": 0,
                ""Certificate"": {
                    ""Source"": ""File"",
                    ""Path"": ""testCert.pfx"",
                    ""Password"": ""testPassword""
                }
            }
        }
    }
}
");
                using (var webHost = WebHost.Start(context => context.Response.WriteAsync("Hello, World!")))
                {
                    var port = GetWebHostPort(webHost);

                    var response = await HttpClientSlim.GetStringAsync($"https://127.0.0.1:{port}", validateCertificate: false);
                    Assert.Equal("Hello, World!", response);
                }
            }
            finally
            {
                File.Delete("appsettings.json");
            }
        }

        [Fact]
        public void LoggingConfigurationSectionPassedToLoggerByDefault()
        {
            try
            {
                File.WriteAllText("appsettings.json", @"
{
    ""Logging"": {
        ""LogLevel"": {
            ""Default"": ""Warning""
        }
    }
}
");
                using (var webHost = WebHost.Start(context => context.Response.WriteAsync("Hello, World!")))
                {
                    var factory = (ILoggerFactory)webHost.Services.GetService(typeof(ILoggerFactory));
                    var logger = factory.CreateLogger("Test");

                    logger.Log(LogLevel.Information, 0, "Message", null, (s, e) =>
                    {
                        Assert.True(false);
                        return string.Empty;
                    });

                    var logWritten = false;
                    logger.Log(LogLevel.Warning, 0, "Message", null, (s, e) =>
                    {
                        logWritten = true;
                        return string.Empty;
                    });

                    Assert.True(logWritten);
                }
            }
            finally
            {
                File.Delete("appsettings.json");
            }
        }

        private async Task ExecuteStartOrStartWithTest(Func<DeploymentResult, Task<HttpResponseMessage>> getResponse, string applicationName)
        {
            await ExecuteTestApp(applicationName, async (deploymentResult, logger) =>
            {
                var response = await RetryHelper.RetryRequest(() => getResponse(deploymentResult), logger, deploymentResult.HostShutdownToken);

                var responseText = await response.Content.ReadAsStringAsync();
                try
                {
                    Assert.Equal(applicationName, responseText);
                }
                catch (XunitException)
                {
                    logger.LogWarning(response.ToString());
                    logger.LogWarning(responseText);
                    throw;
                }
            });
        }

        private async Task ExecuteTestApp(string applicationName, Func<DeploymentResult, ILogger, Task> assertAction, bool setTestEnvVars = false)
        {
            using (StartLog(out var loggerFactory, applicationName))
            {
                var logger = loggerFactory.CreateLogger(nameof(WebHost.Start));
                var deploymentParameters = new DeploymentParameters(Path.Combine(_testSitesPath, applicationName), ServerType.Kestrel, RuntimeFlavor.CoreClr, RuntimeArchitecture.x64);

                if (setTestEnvVars)
                {
                    deploymentParameters.EnvironmentVariables.Add(new KeyValuePair<string, string>("aspnetcore_environment", "Development"));
                    deploymentParameters.EnvironmentVariables.Add(new KeyValuePair<string, string>("envKey", "envValue"));
                }

                using (var deployer = ApplicationDeployerFactory.Create(deploymentParameters, loggerFactory))
                {
                    var deploymentResult = await deployer.DeployAsync();

                    await assertAction(deploymentResult, logger);
                }
            }
        }

        private static string GetTestSitesPath()
        {
            var applicationBasePath = AppContext.BaseDirectory;

            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var solutionFileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, "MetaPackages.sln"));
                if (solutionFileInfo.Exists)
                {
                    return Path.GetFullPath(Path.Combine(directoryInfo.FullName, "test", "TestSites"));
                }

                directoryInfo = directoryInfo.Parent;
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"Solution root could not be found using {applicationBasePath}");
        }

        private static int GetWebHostPort(IWebHost webHost)
            => webHost.ServerFeatures.Get<IServerAddressesFeature>().Addresses
                .Select(serverAddress => new Uri(serverAddress).Port)
                .FirstOrDefault();
    }
}
