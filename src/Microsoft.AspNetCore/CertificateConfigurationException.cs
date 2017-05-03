// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore
{
    internal class CertificateConfigurationException : Exception
    {
        public CertificateConfigurationException(Exception innerException)
            : base($"{innerException.Message} For information on configuring HTTPS see https://go.microsoft.com/fwlink/?linkid=848054.", innerException)
        {
        }
    }
}
