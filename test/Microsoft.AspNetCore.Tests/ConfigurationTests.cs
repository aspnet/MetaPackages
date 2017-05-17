// Copyright (c) .NET Foundation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Identity.Service.IntegratedWebClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Tests
{
    public class ConfigurationTests
    {
        private const string AuthenticationSection = "Microsoft:AspNetCore:Authentication:";
        private const string AuthenticationSchemesSection = "Microsoft:AspNetCore:Authentication:Schemes:";

        [Fact]
        public void IntegratedWebClientCanBindAgainstDefaultConfig()
        {
            var dic = new Dictionary<string, string>
            {
                { AuthenticationSection + "IdentityService:ClientId", "<id>"},
                { AuthenticationSection + "IdentityService:TokenRedirectUrn", "urn:self:aspnet:identity:integrated"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddOptions()
            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptions<IntegratedWebClientOptions>>().Value;
            Assert.Equal("<id>", options.ClientId);
        }

        [Fact]
        public void FacebookCanBindAgainstDefaultConfig()
        {
            var dic = new Dictionary<string, string>
            {
                {AuthenticationSchemesSection + "Facebook:AppId", "<id>"},
                {AuthenticationSchemesSection + "Facebook:AppSecret", "<secret>"},
                {AuthenticationSchemesSection + "Facebook:AuthorizationEndpoint", "<authEndpoint>"},
                {AuthenticationSchemesSection + "Facebook:BackchannelTimeout", "0.0:0:30"},
                {AuthenticationSchemesSection + "Facebook:ClaimsIssuer", "<issuer>"},
                {AuthenticationSchemesSection + "Facebook:RemoteAuthenticationTimeout", "0.0:0:30"},
                {AuthenticationSchemesSection + "Facebook:SaveTokens", "true"},
                {AuthenticationSchemesSection + "Facebook:SendAppSecretProof", "true"},
                {AuthenticationSchemesSection + "Facebook:SignInScheme", "<signIn>"},
                {AuthenticationSchemesSection + "Facebook:TokenEndpoint", "<tokenEndpoint>"},
                {AuthenticationSchemesSection + "Facebook:UserInformationEndpoint", "<userEndpoint>"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .ConfigureAspNetCoreDefaults()
                .AddFacebookAuthentication();
            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsSnapshot<FacebookOptions>>().Get(FacebookDefaults.AuthenticationScheme);
            Assert.Equal("<id>", options.AppId);
            Assert.Equal("<secret>", options.AppSecret);
            Assert.Equal("<authEndpoint>", options.AuthorizationEndpoint);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.BackchannelTimeout);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.True(options.SendAppSecretProof);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<tokenEndpoint>", options.TokenEndpoint);
            Assert.Equal("<userEndpoint>", options.UserInformationEndpoint);
        }

        [Fact]
        public void MicrosoftAccountCanBindAgainstDefaultConfig()
        {
            var dic = new Dictionary<string, string>
            {
                { AuthenticationSchemesSection + "Microsoft:ClientId", "<id>"},
                { AuthenticationSchemesSection + "Microsoft:ClientSecret", "<secret>"},
                { AuthenticationSchemesSection + "Microsoft:AuthorizationEndpoint", "<authEndpoint>"},
                { AuthenticationSchemesSection + "Microsoft:BackchannelTimeout", "0.0:0:30"},
                { AuthenticationSchemesSection + "Microsoft:ClaimsIssuer", "<issuer>"},
                { AuthenticationSchemesSection + "Microsoft:RemoteAuthenticationTimeout", "0.0:0:30"},
                { AuthenticationSchemesSection + "Microsoft:SaveTokens", "true"},
                { AuthenticationSchemesSection + "Microsoft:SendAppSecretProof", "true"},
                { AuthenticationSchemesSection + "Microsoft:SignInScheme", "<signIn>"},
                { AuthenticationSchemesSection + "Microsoft:TokenEndpoint", "<tokenEndpoint>"},
                { AuthenticationSchemesSection + "Microsoft:UserInformationEndpoint", "<userEndpoint>"},
            };
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(dic);
            var config = configurationBuilder.Build();
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .ConfigureAspNetCoreDefaults()
                .AddMicrosoftAccountAuthentication();
            var sp = services.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptionsSnapshot<MicrosoftAccountOptions>>().Get(MicrosoftAccountDefaults.AuthenticationScheme);
            Assert.Equal("<authEndpoint>", options.AuthorizationEndpoint);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.BackchannelTimeout);
            Assert.Equal("<issuer>", options.ClaimsIssuer);
            Assert.Equal("<id>", options.ClientId);
            Assert.Equal("<secret>", options.ClientSecret);
            Assert.Equal(new TimeSpan(0, 0, 0, 30), options.RemoteAuthenticationTimeout);
            Assert.True(options.SaveTokens);
            Assert.Equal("<signIn>", options.SignInScheme);
            Assert.Equal("<tokenEndpoint>", options.TokenEndpoint);
            Assert.Equal("<userEndpoint>", options.UserInformationEndpoint);
        }
    }
}