// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore
{
    /// <summary>
    /// Exposes extension method for establishing configuration defaults.
    /// </summary>
    public static class AspNetCoreExtensions
    {
        // REVIEW: consider making these public?
        internal static readonly string MicrosoftSection = "Microsoft:";
        internal static readonly string AspNetCoreSection = MicrosoftSection + "AspNetCore:";
        internal static readonly string AuthenticationSchemesSection = AspNetCoreSection + "Authentication:Schemes:";
        internal static readonly string HostingSection = AspNetCoreSection + "Hosting:";

        /// <summary>
        /// Uses the default configuration to extablish defaults.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to modify.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection ConfigureAspNetCoreDefaults(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>();
            services.AddTransient<IConfigureOptions<IdentityOptions>, IdentityOptionsSetup>();
            services.AddTransient<IConfigureOptions<FacebookOptions>, FacebookOptionsSetup>();
            services.AddTransient<IConfigureOptions<GoogleOptions>, GoogleOptionsSetup>();
            services.AddTransient<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();
            services.AddTransient<IConfigureOptions<MicrosoftAccountOptions>, MicrosoftAccountOptionsSetup>();
            services.AddTransient<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectOptionsSetup>();
            services.AddTransient<IConfigureOptions<TwitterOptions>, TwitterOptionsSetup>();
            return services;
        }

        private class IdentityOptionsSetup : ConfigureOptions<IdentityOptions>
        {
            public IdentityOptionsSetup(IConfiguration config) :
                base(options => config.GetSection(MicrosoftSection+"Identity").Bind(options))
            { }
        }

        private class FacebookOptionsSetup : ConfigureNamedOptions<FacebookOptions>
        {
            public FacebookOptionsSetup(IConfiguration config) :
                base(FacebookDefaults.AuthenticationScheme,
                    options => config.GetSection(AuthenticationSchemesSection + FacebookDefaults.AuthenticationScheme).Bind(options))
            { }
        }

        private class GoogleOptionsSetup : ConfigureNamedOptions<GoogleOptions>
        {
            public GoogleOptionsSetup(IConfiguration config) :
                base(GoogleDefaults.AuthenticationScheme,
                    options => config.GetSection(AuthenticationSchemesSection + GoogleDefaults.AuthenticationScheme).Bind(options))
            { }
        }

        private class JwtBearerOptionsSetup : ConfigureNamedOptions<JwtBearerOptions>
        {
            public JwtBearerOptionsSetup(IConfiguration config) :
                base(JwtBearerDefaults.AuthenticationScheme,
                    options => config.GetSection(AuthenticationSchemesSection + JwtBearerDefaults.AuthenticationScheme).Bind(options))
            { }
        }

        private class MicrosoftAccountOptionsSetup : ConfigureNamedOptions<MicrosoftAccountOptions>
        {
            public MicrosoftAccountOptionsSetup(IConfiguration config) :
                base(MicrosoftAccountDefaults.AuthenticationScheme,
                    options => config.GetSection(AuthenticationSchemesSection + MicrosoftAccountDefaults.AuthenticationScheme).Bind(options))
            { }
        }

        private class OpenIdConnectOptionsSetup : ConfigureNamedOptions<OpenIdConnectOptions>
        {
            public OpenIdConnectOptionsSetup(IConfiguration config) :
                base(OpenIdConnectDefaults.AuthenticationScheme,
                    options => config.GetSection(AuthenticationSchemesSection + OpenIdConnectDefaults.AuthenticationScheme).Bind(options))
            { }
        }

        private class TwitterOptionsSetup : ConfigureNamedOptions<TwitterOptions>
        {
            public TwitterOptionsSetup(IConfiguration config) :
                base(TwitterDefaults.AuthenticationScheme,
                    options => config.GetSection(AuthenticationSchemesSection + TwitterDefaults.AuthenticationScheme).Bind(options))
            { }
        }

    }
}
