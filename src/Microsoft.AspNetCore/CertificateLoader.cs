// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore
{
    /// <summary>
    /// A helper class to load certificates from files and certificate stores based on <seealso cref="IConfiguration"/> data.
    /// </summary>
    public class CertificateLoader
    {
        private readonly IConfiguration _certificatesConfiguration;

        /// <summary>
        /// Creates a new instance of <see cref="CertificateLoader"/>.
        /// </summary>
        public CertificateLoader()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="CertificateLoader"/> that can load certificates by name.
        /// </summary>
        /// <param name="certificatesConfig">An <see cref="IConfiguration"/> instance with information about certificates.</param>
        public CertificateLoader(IConfiguration certificatesConfig)
        {
            _certificatesConfiguration = certificatesConfig;
        }

        /// <summary>
        /// Loads one or more certificates based on the information found in a configuration section.
        /// </summary>
        /// <param name="certificateConfiguration">A configuration section containing either a string value referencing certificates
        /// by name, or one or more inline certificate specifications.
        /// </param>
        /// <returns>One or more loaded certificates.</returns>
        public IEnumerable<X509Certificate2> Load(IConfigurationSection certificateConfiguration)
        {
            var certificateNames = certificateConfiguration.Value;

            List<X509Certificate2> certificates = new List<X509Certificate2>();

            if (certificateNames != null)
            {
                foreach (var certificateName in certificateNames.Split(' '))
                {
                    certificates.Add(Load(certificateName));
                }
            }
            else
            {
                if (certificateConfiguration["Source"] != null)
                {
                    certificates.Add(LoadSingle(certificateConfiguration));
                }
                else
                {
                    certificates.AddRange(LoadMultiple(certificateConfiguration));
                }
            }

            return certificates;
        }

        /// <summary>
        /// Loads a certificate by name.
        /// </summary>
        /// <param name="certificateName">The certificate name.</param>
        /// <returns>The loaded certificate</returns>
        /// <remarks>This method only works if the <see cref="CertificateLoader"/> instance was constructed with
        /// a reference to an <see cref="IConfiguration"/> instance containing named certificates.
        /// </remarks>
        public X509Certificate2 Load(string certificateName)
        {
            var certificateConfiguration = _certificatesConfiguration?.GetSection(certificateName);

            if (certificateConfiguration == null)
            {
                throw new InvalidOperationException($"No certificate named {certificateName} found in configuration");
            }

            return LoadSingle(certificateConfiguration);
        }

        private X509Certificate2 LoadSingle(IConfigurationSection certificateConfiguration)
        {
            var sourceKind = certificateConfiguration["Source"];

            CertificateSource certificateSource;
            switch (sourceKind.ToLowerInvariant())
            {
                case "file":
                    certificateSource = new CertificateFileSource();
                    break;
                case "store":
                    certificateSource = new CertificateStoreSource();
                    break;
                default:
                    throw new InvalidOperationException($"Invalid certificate source kind: {sourceKind}");
            }

            certificateConfiguration.Bind(certificateSource);

            return certificateSource.Load();
        }

        private IEnumerable<X509Certificate2> LoadMultiple(IConfigurationSection certificatesConfiguration)
            => certificatesConfiguration.GetChildren().Select(LoadSingle);

        private abstract class CertificateSource
        {
            public string Source { get; set; }

            public abstract X509Certificate2 Load();
        }

        private class CertificateFileSource : CertificateSource
        {
            public string Path { get; set; }

            public string Password { get; set; }

            public override X509Certificate2 Load()
            {
                Exception error;
                var certificate =
#if NETCOREAPP2_0
                    TryLoad(X509KeyStorageFlags.EphemeralKeySet, out error) ??
#endif
                    TryLoad(X509KeyStorageFlags.UserKeySet, out error);

                if (error != null)
                {
                    throw error;
                }

                return certificate;
            }

            private X509Certificate2 TryLoad(X509KeyStorageFlags flags, out Exception exception)
            {
                try
                {
                    var loadedCertificate = new X509Certificate2(Path, Password, flags);
                    exception = null;
                    return loadedCertificate;
                }
                catch (Exception e)
                {
                    exception = e;
                    return null;
                }
            }
        }

        private class CertificateStoreSource : CertificateSource
        {
            public string Subject { get; set; }
            public string StoreName { get; set; }
            public string StoreLocation { get; set; }
            public bool AllowInvalid { get; set; }

            public override X509Certificate2 Load()
            {
                if (!Enum.TryParse(StoreLocation, ignoreCase: true, result: out StoreLocation storeLocation))
                {
                    throw new InvalidOperationException($"Invalid store location: {StoreLocation}");
                }

                using (var store = new X509Store(StoreName, storeLocation))
                {
                    X509Certificate2Collection storeCertificates = null;
                    X509Certificate2Collection foundCertificates = null;
                    X509Certificate2 foundCertificate = null;

                    try
                    {
                        store.Open(OpenFlags.ReadOnly);
                        storeCertificates = store.Certificates;
                        foundCertificates = storeCertificates.Find(X509FindType.FindBySubjectDistinguishedName, Subject, validOnly: !AllowInvalid);
                        foundCertificate = foundCertificates
                            .OfType<X509Certificate2>()
                            .OrderByDescending(certificate => certificate.NotAfter)
                            .FirstOrDefault();

                        if (foundCertificate == null)
                        {
                            throw new InvalidOperationException($"No certificate found for {Subject} in store {StoreName} in {StoreLocation}");
                        }

                        return foundCertificate;
                    }
                    finally
                    {
                        if (foundCertificate != null)
                        {
                            storeCertificates.Remove(foundCertificate);
                            foundCertificates.Remove(foundCertificate);
                        }

                        DisposeCertificates(storeCertificates);
                        DisposeCertificates(foundCertificates);
                    }
                }
            }

            private void DisposeCertificates(X509Certificate2Collection certificates)
            {
                if (certificates != null)
                {
                    foreach (var certificate in certificates)
                    {
                        certificate.Dispose();
                    }
                }
            }
        }
    }
}
