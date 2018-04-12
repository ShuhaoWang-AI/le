using System;
using System.Collections.Generic;
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
            _logger.Info(message:"Start: CertificateDigitalSignatureManager.SignData.");
            var dataBytes = Convert.FromBase64String(s:base64Data);
            var signature = GetDataSignature(data:dataBytes);
            _logger.Info(message:"End: CertificateDigitalSignatureManager.SignData.");

            return signature;
        }

        public bool VerifySignature(string currentSignatureStr, byte[] dataToVerify, int copyOfRecordCertificateId)
        {
            _logger.Info(message:"Start: CertificateDigitalSignatureManager.VerifySignature.");
            try
            {
                var dataToVerifyHash = HashHelper.ComputeSha256Hash(data:dataToVerify);
                var dataToVerifyHashBytes = Encoding.UTF8.GetBytes(s:dataToVerifyHash);
                var signedDataBytes = Convert.FromBase64String(s:currentSignatureStr);
                
                _logger.Info(message:"End: CertificateDigitalSignatureManager.VerifySignature.");

                return VerifySignature(originData:dataToVerifyHashBytes, signedData:signedDataBytes, certificateId:copyOfRecordCertificateId);
            }
            catch (Exception ex)
            {
                var errors = new List<string> {ex.Message};
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    errors.Add(item:ex.Message);
                }

                _logger.Error(message:"Error: CertificateDigitalSignatureManager.VerifySignature. {0} ", argument:string.Join(separator:"," + Environment.NewLine, values:errors));
                return false;
            }
        }

        public int GetLatestCertificateId()
        {
           InitializeCertificate();
            return _currentCertificateId;
        }

        #endregion

        #region private section

        private CopyOfRecordCertificate GetCertificate(int? certificateId)
        {
            if (certificateId.HasValue)
            {
                return _dbContext.CopyOfRecordCertificates.Single(t => t.CopyOfRecordCertificateId == certificateId);
            } 
            else 
            {
              // Use the latest one
              return _dbContext.CopyOfRecordCertificates.OrderByDescending(t => t.CreationDateTimeUtc).First();  
            } 
        }

        private string GetDataSignature(byte[] data)
        {
            return Convert.ToBase64String(inArray:SignData(data:data));
        }

        private byte[] SignData(byte[] data)
        {
            InitializeCertificate();
            var halg = new SHA1CryptoServiceProvider();
            return _privateKey.SignData(buffer:data, halg:halg);
        }

        private bool VerifySignature(byte[] originData, byte[] signedData, int certificateId)
        {
            InitializeCertificate(certificateId);
            var halg = new SHA1CryptoServiceProvider();
            return _publicKey.VerifyData(buffer:originData, halg:halg, signature:signedData);
        }

        private void InitializeCertificate(int? certificateId = null)
        {
            var certificateInfo = GetCertificate(certificateId);
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
        } 

        #endregion
    }
}