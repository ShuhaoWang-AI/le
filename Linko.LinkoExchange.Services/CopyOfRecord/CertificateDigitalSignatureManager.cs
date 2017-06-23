using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Mapping;
using NLog;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public class CertificateDigitalSignatureManager : IDigitalSignatureManager
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;

        private RSACryptoServiceProvider _privateKey;
        private RSACryptoServiceProvider _publicKey;
        private int _currentCertificateId;
        private bool _initialized = false;
        private readonly object _lock = new object();


        public CertificateDigitalSignatureManager(
            LinkoExchangeContext dbContext,
            ILogger logger,
            IHttpContextService httpContextService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
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
            _logger = logger;
        }

        public string SignData(string base64Data)
        {
            _logger.Info("Enter CertificateDigitalSignatureManager.GetDataSignature.");
            var dataBytes = Convert.FromBase64String(base64Data);
            var signature = GetDataSignature(dataBytes);
            _logger.Info("Leave CertificateDigitalSignatureManager.GetDataSignature.");

            return signature;
        }

        public bool VerifySignature(string currentSignatureStr, byte[] dataToVerify)
        {
            _logger.Info("Enter CertificateDigitalSignatureManager.VerifySignature.");
            try
            {
                var dataToVerifyHash = HashHelper.ComputeSha256Hash(dataToVerify);
                var dataToVerifyHashBytes = Encoding.UTF8.GetBytes(dataToVerifyHash);
                var signedDataBytes = Convert.FromBase64String(currentSignatureStr);

                return VerifySignature(dataToVerifyHashBytes, signedDataBytes);
            }
            catch (Exception ex)
            {
                _logger.Info($"Error happens in CertificateDigitalSignatureManager.VerifySignature.  Error: {ex.Message}");
                return false;
            } 
        }

        public int LatestCertificateId()
        {
            if (_initialized == false)
            {
                InitializeCertificate();
            }

            return _currentCertificateId;
        }

        #region private section
        private CopyOfRecordCertificate GetLatestCertificate()
        {
            return _dbContext.CopyOfRecordCertificates.OrderByDescending(t => t.CreationDateTimeUtc).First();
        }

        private string GetDataSignature(byte[] data)
        {
            return Convert.ToBase64String(this.SignData(data));
        }

        private byte[] SignData(byte[] data)
        {
            if (_initialized == false)
            {
                InitializeCertificate();
            }

            var halg = new SHA1CryptoServiceProvider();
            return _privateKey.SignData(data, halg);
        }
        private bool VerifySignature(byte[] originData, byte[] signedData)
        {
            if (_initialized == false)
            {
                InitializeCertificate();
            }

            var halg = new SHA1CryptoServiceProvider();
            return _publicKey.VerifyData(originData, halg, signedData);
        }

        private void InitializeCertificate()
        {
            var certificateInfo = GetLatestCertificate();
            _currentCertificateId = certificateInfo.CopyOfRecordCertificateId;

            var certificatePassword = certificateInfo.Password;
            var certifcateFile = Path.Combine(certificateInfo.PhysicalPath, certificateInfo.FileName);
            X509Certificate2 certificate;
            lock(_lock)
            {
                certificate = new X509Certificate2(certifcateFile, certificatePassword);
            }
            
            _privateKey = (RSACryptoServiceProvider)certificate.PrivateKey;
            _publicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;
            
            if (_privateKey == null || _publicKey == null)
            {
                throw new Exception(message: "Invalid certificate or password.");
            }

            _initialized = true;
      }

        #endregion 
    }
}