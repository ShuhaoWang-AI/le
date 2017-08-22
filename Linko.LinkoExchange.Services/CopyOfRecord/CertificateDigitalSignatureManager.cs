using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.HttpContext;
using NLog;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public class CertificateDigitalSignatureManager : IDigitalSignatureManager
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly object _lock = new object();
        private readonly ILogger _logger;
        private int _currentCertificateId;
        private bool _initialized;

        private RSACryptoServiceProvider _privateKey;
        private RSACryptoServiceProvider _publicKey;

        #endregion

        #region constructors and destructor

        public CertificateDigitalSignatureManager(
            LinkoExchangeContext dbContext,
            ILogger logger,
            IHttpContextService httpContextService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(paramName:nameof(dbContext));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(paramName:nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(paramName:nameof(httpContextService));
            }

            _dbContext = dbContext;
            _logger = logger;
        }

        #endregion

        #region interface implementations

        public string SignData(string base64Data)
        {
            _logger.Info(message:"Enter CertificateDigitalSignatureManager.GetDataSignature.");
            var dataBytes = Convert.FromBase64String(s:base64Data);
            var signature = GetDataSignature(data:dataBytes);
            _logger.Info(message:"Leave CertificateDigitalSignatureManager.GetDataSignature.");

            return signature;
        }

        public bool VerifySignature(string currentSignatureStr, byte[] dataToVerify)
        {
            _logger.Info(message:"Enter CertificateDigitalSignatureManager.VerifySignature.");
            try
            {
                var dataToVerifyHash = HashHelper.ComputeSha256Hash(data:dataToVerify);
                var dataToVerifyHashBytes = Encoding.UTF8.GetBytes(s:dataToVerifyHash);
                var signedDataBytes = Convert.FromBase64String(s:currentSignatureStr);

                return VerifySignature(originData:dataToVerifyHashBytes, signedData:signedDataBytes);
            }
            catch (Exception ex)
            {
                _logger.Info(message:$"Error happens in CertificateDigitalSignatureManager.VerifySignature.  Error: {ex.Message}");
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

        #endregion

        #region private section

        private CopyOfRecordCertificate GetLatestCertificate()
        {
            return _dbContext.CopyOfRecordCertificates.OrderByDescending(t => t.CreationDateTimeUtc).First();
        }

        private string GetDataSignature(byte[] data)
        {
            return Convert.ToBase64String(inArray:SignData(data:data));
        }

        private byte[] SignData(byte[] data)
        {
            if (_initialized == false)
            {
                InitializeCertificate();
            }

            var halg = new SHA1CryptoServiceProvider();
            return _privateKey.SignData(buffer:data, halg:halg);
        }

        private bool VerifySignature(byte[] originData, byte[] signedData)
        {
            if (_initialized == false)
            {
                InitializeCertificate();
            }

            var halg = new SHA1CryptoServiceProvider();
            return _publicKey.VerifyData(buffer:originData, halg:halg, signature:signedData);
        }

        private void InitializeCertificate()
        {
            var certificateInfo = GetLatestCertificate();
            _currentCertificateId = certificateInfo.CopyOfRecordCertificateId;

            var certificatePassword = certificateInfo.Password;
            var certifcateFile = Path.Combine(path1:certificateInfo.PhysicalPath, path2:certificateInfo.FileName);
            X509Certificate2 certificate;
            lock (_lock)
            {
                certificate = new X509Certificate2(fileName:certifcateFile, password:certificatePassword);
            }

            _privateKey = (RSACryptoServiceProvider) certificate.PrivateKey;
            _publicKey = (RSACryptoServiceProvider) certificate.PublicKey.Key;

            if (_privateKey == null || _publicKey == null)
            {
                throw new Exception(message:"Invalid certificate or password.");
            }

            _initialized = true;
        }

        #endregion
    }
}