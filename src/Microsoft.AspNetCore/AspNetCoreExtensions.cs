// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options.Infrastructure;

namespace Microsoft.AspNetCore
{
    /// <summary>
    /// Exposes extension method for establishing configuration defaults.
    /// </summary>
    public static class AspNetCoreExtensions
    {
        /// <summary>
        /// Enables features to uses the default configuration to extablish defaults for their options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to modify.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection ConfigureAspNetCoreDefaults(this IServiceCollection services)
        {
            services.AddTransient(typeof(IConfigureOptions<>), typeof(ConfigureDefaults<>));
            services.AddTransient(typeof(IConfigureNamedOptions<>), typeof(ConfigureDefaults<>));
            return services;
        }
    }
}