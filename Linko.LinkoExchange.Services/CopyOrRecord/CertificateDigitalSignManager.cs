using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public class CertificateDigitalSignManager : IDigitalSignManager
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IMapHelper _mapHelper;
        private readonly ILogger _logger;

        private readonly X509Certificate2 _certificate;
        private readonly RSACryptoServiceProvider _privateKey;
        private readonly RSACryptoServiceProvider _publicKey;

        private readonly int _currentCertificateId;

        public CertificateDigitalSignManager(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (mapHelper == null)
            {
                throw new ArgumentNullException(nameof(mapHelper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(nameof(httpContextService));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;

            var certificateInfo = GetLatestCertificate();
            _currentCertificateId = certificateInfo.CopyOfRecordCertificateId;

            var certificatePassword = certificateInfo.Password;
            var certifcateFile = Path.Combine(certificateInfo.PhysicalPath, certificateInfo.FileName);

            _certificate = new X509Certificate2(certifcateFile, certificatePassword);
            _privateKey = (RSACryptoServiceProvider)_certificate.PrivateKey;
            _publicKey = (RSACryptoServiceProvider)_certificate.PublicKey.Key;

            if (_privateKey == null || _publicKey == null)
            {
                throw new Exception(message: "Invalid certificate or password.");
            }
        }

        public string SignData(string base64Data)
        {
            _logger.Info("Enter CertificateDigitalSignManager.GetDataSignature.");
            var dataBytes = Convert.FromBase64String(base64Data);
            var signature = GetDataSignature(dataBytes);
            _logger.Info("Leave CertificateDigitalSignManager.GetDataSignature.");

            return signature;
        }

        public virtual CopyOfRecordCertificate GetLatestCertificate()
        {
            return _dbContext.CopyOfRecordCertificates.OrderByDescending(t => t.CreationDateTimeUtc).First();
        }

        #region private section
        private string GetDataSignature(byte[] data)
        {
            return Convert.ToBase64String(this.SignData(data));
        }

        private byte[] SignData(byte[] data)
        {
            var halg = new SHA1CryptoServiceProvider();
            return _privateKey.SignData(data, halg);
        }

        #endregion 
    }
}