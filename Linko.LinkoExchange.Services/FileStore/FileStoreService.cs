using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Configuration;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.FileStore
{
    public class FileStoreService : BaseService, IFileStoreService
    {
        #region static fields and constants

        private const int SizeToReduce = 1024 * 1024 * 2;

        // Max file size 10 M Byte 
        private static readonly int MaxFileSize = ((HttpRuntimeSection) ConfigurationManager.GetSection(sectionName:"system.web/httpRuntime")).MaxRequestLength * 1024;

        #endregion

        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly ITimeZoneService _timeZoneService;

        #endregion

        #region constructors and destructor

        public FileStoreService(
            LinkoExchangeContext dbContext,
            IMapHelper mapHelper,
            ILogger logger,
            IHttpContextService httpContextService,
            ITimeZoneService timeZoneService)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(paramName:nameof(dbContext));
            }

            if (mapHelper == null)
            {
                throw new ArgumentNullException(paramName:nameof(mapHelper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(paramName:nameof(logger));
            }

            if (httpContextService == null)
            {
                throw new ArgumentNullException(paramName:nameof(httpContextService));
            }

            if (timeZoneService == null)
            {
                throw new ArgumentNullException(paramName:nameof(timeZoneService));
            }

            _dbContext = dbContext;
            _mapHelper = mapHelper;
            _logger = logger;
            _httpContextService = httpContextService;
            _timeZoneService = timeZoneService;
        }

        #endregion

        #region interface implementations

        public List<string> GetValidAttachmentFileExtensions()
        {
            _logger.Info(message:"Enter FileStoreService.GetValidAttachmentFileExtensions.");

            var fileTypes = _dbContext.FileTypes.Select(i => i.Extension);

            _logger.Info(message:"Leave FileStoreService.GetValidAttachmentFileExtensions.");
            return new List<string>(collection:fileTypes);
        }

        // Here should validate data from tFileType table
        public bool IsValidFileExtension(string ext)
        {
            if (string.IsNullOrWhiteSpace(value:ext))
            {
                return false;
            }

            if (!ext.StartsWith(value:"."))
            {
                ext = $".{ext}";
            }
            var fileTypes = _dbContext.FileTypes.Select(i => i.Extension).Select(i => i.ToLower());
            return fileTypes.Contains(item:ext.ToLower());
        }

        public List<FileStoreDto> GetFileStores()
        {
            _logger.Info(message:"Enter FileStoreService.GetFileStores.");

            var currentRegulatoryProgramId =
                int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var fileStores =
                _dbContext.FileStores.Where(i => i.OrganizationRegulatoryProgramId == currentRegulatoryProgramId)
                          .Include(t => t.OrganizationRegulatoryProgram).ToList();

            var fileStoreDtos = fileStores.Select(i => _mapHelper.GetFileStoreDtoFromFileStore(fileStore:i)).ToList();

            fileStoreDtos = fileStoreDtos.Select(i => FileStoreDtoHelper(fileStoreDto:i, currentOrgRegProgramId:currentRegulatoryProgramId)).ToList();

            _logger.Info(message:"Leave FileStoreService.GetFileStores.");
            return fileStoreDtos;
        }

        public int CreateFileStore(FileStoreDto fileStoreDto)
        {
            _logger.Info(message:"Enter FileStoreService.CreateFileStore.");

            if (fileStoreDto.Data == null || fileStoreDto.Data.Length < 1)
            {
                var validationIssues = new List<RuleViolation>();
                var message = "No file was selected.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            if (fileStoreDto.Data.Length > MaxFileSize)
            {
                var validationIssues = new List<RuleViolation>();

                var message = $"The file size exceeds that {MaxFileSize / 1024 / 1024} MB limit.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            var extension = Path.GetExtension(path:fileStoreDto.OriginalFileName)?.ToLower();
            var validFileTypes = _dbContext.FileTypes.ToList();
            var validFileExtensions = validFileTypes.Select(i => i.Extension).Select(i => i.ToLower());

            if (!validFileExtensions.Contains(value:extension))
            {
                var validationIssues = new List<RuleViolation>();

                var message = "The file type selected is not supported.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
                    var currentRegulatoryProgramId =
                        int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

                    // Try to reduce the image file size  
                    if (IsImageFile(extension:extension) && fileStoreDto.Data.Length > SizeToReduce)
                    {
                        TryToReduceFileDataSize(fileStoreDto:fileStoreDto);
                    }

                    fileStoreDto.OrganizationRegulatoryProgramId = currentRegulatoryProgramId;
                    fileStoreDto.UploaderUserId = currentUserId;
                    fileStoreDto.SizeByte = fileStoreDto.Data.Length;
                    fileStoreDto.FileTypeId = validFileTypes.Single(i => i.Extension.ToLower().Equals(value:extension)).FileTypeId;
                    var fileName = Path.GetFileNameWithoutExtension(path:fileStoreDto.OriginalFileName);
                    var timeTick = DateTimeOffset.UtcNow.Ticks.ToString();
                    fileStoreDto.Name = $"{fileName}_{timeTick}_{extension}";

                    var fileStore = _mapHelper.GetFileStoreFromFileStoreDto(fileStoreDto:fileStoreDto);

                    fileStore.UploadDateTimeUtc = DateTimeOffset.Now;

                    fileStore = _dbContext.FileStores.Add(entity:fileStore);
                    _dbContext.SaveChanges();

                    // File name is the file name plus id plus extension
                    fileStore.Name = $"{fileName}_{fileStore.FileStoreId}{extension}";
                    var fileStoreData = new FileStoreData
                                        {
                                            Data = fileStoreDto.Data,
                                            FileStoreId = fileStore.FileStoreId
                                        };

                    _dbContext.FileStoreDatas.Add(entity:fileStoreData);
                    _dbContext.SaveChanges();
                    _dbContext.Commit(transaction:transaction);
                    _logger.Info(message:"Leave FileStoreService.CreateFileStore.");

                    return fileStore.FileStoreId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var errors = new List<string> {ex.Message};
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(item:ex.Message);
                    }

                    _logger.Error(message:"Error happens {0} ", argument:string.Join(separator:"," + Environment.NewLine, values:errors));
                    throw;
                }
            }
        }

        public FileStoreDto GetFileStoreById(int fileStoreId, bool includingFileData = false)
        {
            _logger.Info(message:"Enter FileStoreService.GetFileStoreById, attachmentFileId={0}.", argument:fileStoreId);

            if (!CanUserExecuteApi(id:fileStoreId))
            {
                throw new UnauthorizedAccessException();
            }

            var currentRegulatoryProgramId =
                int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var fileStore =
                _dbContext.FileStores.Single(i => i.FileStoreId == fileStoreId);

            var fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore:fileStore);
            if (includingFileData)
            {
                fileStoreDto.Data = _dbContext.FileStoreDatas.Single(i => i.FileStoreId == fileStoreDto.FileStoreId.Value).Data;
            }

            FileStoreDtoHelper(fileStoreDto:fileStoreDto, currentOrgRegProgramId:currentRegulatoryProgramId);
            _logger.Info(message:"Leave FileStoreService.GetFileStoreById, attachmentFileId={0}.", argument:fileStoreId);

            return fileStoreDto;
        }

        public void UpdateFileStore(FileStoreDto fileStoreDto)
        {
            _logger.Info(message:"Enter FileStoreService.UpdateFileStore.");

            if (fileStoreDto == null || fileStoreDto.FileStoreId.HasValue == false)
            {
                _logger.Info(message:"Leave FileStoreService.UpdateFileStore. null filsStoreDto or fileStoreDto.FileStoreId");
                return;
            }

            if (!CanUserExecuteApi(id:fileStoreDto.FileStoreId.Value))
            {
                throw new UnauthorizedAccessException();
            }

            //Check if the file is already set to be 'reported' or not 
            var fileStoreToUpdate = _dbContext.FileStores.Single(i => i.FileStoreId == fileStoreDto.FileStoreId);

            // Check the fileStoreId is in tReportFile table or not, if it, means that file is included in a report  
            if (IsFileInReports(fileStoreId:fileStoreDto.FileStoreId.Value))
            {
                var message = "The attachment is used in a Report Package, and cannot be changed.";
                var validationIssues = new List<RuleViolation>();
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
                    fileStoreToUpdate.LastModifierUserId = currentUserId;
                    fileStoreToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;

                    fileStoreToUpdate.Description = fileStoreDto.Description;
                    fileStoreToUpdate.ReportElementTypeName = fileStoreDto.ReportElementTypeName;
                    fileStoreToUpdate.ReportElementTypeId = fileStoreDto.ReportElementTypeId;

                    _dbContext.SaveChanges();
                    _dbContext.Commit(transaction:transaction);
                    _logger.Info(message:"Leave FileStoreService.UpdateFileStore.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var errors = new List<string> {ex.Message};
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(item:ex.Message);
                    }

                    _logger.Error(message:"Error happens {0} ", argument:string.Join(separator:"," + Environment.NewLine, values:errors));
                    throw;
                }
            }
        }

        public void DeleteFileStore(int fileStoreId)
        {
            _logger.Info(message:"Enter FileStoreService.DeleteFileStore.");

            if (!CanUserExecuteApi(id:fileStoreId))
            {
                throw new UnauthorizedAccessException();
            }

            //Check if the file is already set to be 'reported' or not 
            if (IsFileInReports(fileStoreId:fileStoreId))
            {
                var validationIssues = new List<RuleViolation>();
                var message = "The attachment is used in a Report Package, and cannot be changed.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var testFileStore = _dbContext.FileStores.Single(i => i.FileStoreId == fileStoreId);
                    var fileStoreData = _dbContext.FileStoreDatas.Single(i => i.FileStoreId == fileStoreId);
                    _dbContext.FileStoreDatas.Remove(entity:fileStoreData);
                    _dbContext.FileStores.Remove(entity:testFileStore);
                    _dbContext.SaveChanges();
                    _dbContext.Commit(transaction:transaction);
                    _logger.Info(message:"Leave FileStoreService.DeleteFileStore.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var errors = new List<string> {ex.Message};
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(item:ex.Message);
                    }

                    _logger.Error(message:"Error happens {0} ", argument:string.Join(separator:"," + Environment.NewLine, values:errors));
                    throw;
                }
            }
        }

        public bool IsFileInReports(int fileStoreId)
        {
            _logger.Info(message:"Enter FileStoreService.IsFileInReports.");

            var isFileInReports = _dbContext.ReportFiles.Any(i => i.FileStoreId == fileStoreId);

            _logger.Info(message:"Leave FileStoreService.IsFileInReports.");

            return isFileInReports;
        }

        public int GetMaxFileSize()
        {
            return MaxFileSize;
        }

        #endregion

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            var retVal = false;

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentPortalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "UpdateFileStore":
                case "DeleteFileStore":
                {
                    var fileStoreId = id[0];
                    retVal = IsFileStoreWithThisOwnerExist(fileStoreId:fileStoreId, orgRegProgramId:currentOrgRegProgramId);
                }
                    break;

                case "GetFileStoreById":
                {
                    var fileStoreId = id[0];
                    if (currentPortalName.Equals(value:"authority"))
                    {
                        var fileStore = _dbContext.FileStores
                                                  .Include(fs => fs.OrganizationRegulatoryProgram)
                                                  .Include(fs => fs.OrganizationRegulatoryProgram.RegulatorOrganization)
                                                  .SingleOrDefault(fs => fs.FileStoreId == fileStoreId);

                        if (fileStore != null)
                        {
                            //check if current user is authority of file store owner
                            var authorityOrganization = fileStore.OrganizationRegulatoryProgram.RegulatorOrganization ?? fileStore.OrganizationRegulatoryProgram.Organization;

                            var currentOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                                                 .Single(orpu => orpu.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                            var isCurrentOrgRegProgramTheAuthority = currentOrgRegProgram.OrganizationId == authorityOrganization.OrganizationId
                                                                     && currentOrgRegProgram.RegulatoryProgramId == fileStore.OrganizationRegulatoryProgram.RegulatoryProgramId;

                            if (isCurrentOrgRegProgramTheAuthority)
                            {
                                var isFileIncludedInSubmittedReport = _dbContext.FileStores
                                                                                .Include(fs => fs.ReportFiles.Select(rf => rf.ReportPackageElementType.ReportPackageElementCategory
                                                                                                                             .ReportPackage.ReportStatus))
                                                                                .Single(fs => fs.FileStoreId == fileStoreId)
                                                                                .ReportFiles.Select(rf => rf.ReportPackageElementType)
                                                                                .Select(rpet => rpet.ReportPackageElementCategory)
                                                                                .Select(rpec => rpec.ReportPackage)
                                                                                .Any(rp => rp.ReportStatus.Name == ReportStatusName.Submitted.ToString()
                                                                                           || rp.ReportStatus.Name == ReportStatusName.Repudiated.ToString());

                                retVal = isFileIncludedInSubmittedReport;
                            }
                        }
                    }
                    else
                    {
                        retVal = IsFileStoreWithThisOwnerExist(fileStoreId:fileStoreId, orgRegProgramId:currentOrgRegProgramId);
                    }
                }
                    break;

                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        private bool IsFileStoreWithThisOwnerExist(int fileStoreId, int orgRegProgramId)
        {
            //Also handles scenarios where FileStoreId does not exist
            return _dbContext.FileStores
                             .Any(fs => fs.FileStoreId == fileStoreId
                                        && fs.OrganizationRegulatoryProgramId == orgRegProgramId);
        }

        private FileStoreDto FileStoreDtoHelper(FileStoreDto fileStoreDto, int currentOrgRegProgramId)
        {
            fileStoreDto.UploadDateTimeLocal =
                _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:fileStoreDto.UploadDateTimeLocal, orgRegProgramId:currentOrgRegProgramId);

            fileStoreDto.UploaderUserFullName = GetUserFullName(userId:fileStoreDto.UploaderUserId);
            fileStoreDto.LastModifierUserFullName = GetUserFullName(userId:fileStoreDto.LastModifierUserId);

            fileStoreDto.LastModificationDateTimeLocal = fileStoreDto.UploadDateTimeLocal;
            fileStoreDto.LastModificationDateTimeLocal =
                _timeZoneService.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:fileStoreDto.LastModificationDateTimeLocal.Value, orgRegProgramId:currentOrgRegProgramId);

            if (fileStoreDto.FileStoreId.HasValue && IsFileInReports(fileStoreId:fileStoreDto.FileStoreId.Value))
            {
                if (_dbContext.ReportFiles.Any(i => i.FileStoreId == fileStoreDto.FileStoreId))
                {
                    fileStoreDto.UsedByReports = true;
                }
            }

            return fileStoreDto;
        }

        private string GetUserFullName(int? userId)
        {
            if (userId.HasValue)
            {
                var uploaderUser = _dbContext.Users.Single(user => user.UserProfileId == userId.Value);
                return $"{uploaderUser.FirstName} {uploaderUser.LastName}";
            }

            return "N/A";
        }

        private void TryToReduceFileDataSize(FileStoreDto fileStoreDto)
        {
            using (var stream = new MemoryStream(buffer:fileStoreDto.Data))
            {
                var oldImage = new Bitmap(stream:stream);
                var newImage = new Bitmap(width:oldImage.Width, height:oldImage.Height);

                var graphic = Graphics.FromImage(image:newImage);
                graphic.InterpolationMode = InterpolationMode.Low;
                graphic.DrawImage(image:oldImage, x:0, y:0, width:oldImage.Width, height:oldImage.Height);
                graphic.Dispose();

                // transfer newImage into byte;  
                var extension = Path.GetExtension(path:fileStoreDto.OriginalFileName);
                var format = GetNormalizedImageFormat(extension:extension);
                var newImageData = ImageToByte(img:newImage, format:format);
                if (newImageData.Length < fileStoreDto.Data.Length)
                {
                    fileStoreDto.Data = new byte[newImageData.Length];
                    Array.Copy(sourceArray:newImageData, sourceIndex:0, destinationArray:fileStoreDto.Data, destinationIndex:0, length:newImageData.Length);
                }
            }
        }

        private ImageFormat GetNormalizedImageFormat(string extension)
        {
            if (!extension.StartsWith(value:"."))
            {
                extension = $".{extension}";
            }

            extension = extension.ToUpper();

            var format = ImageFormat.Jpeg;
            if (extension == ".BMP")
            {
                format = ImageFormat.Bmp;
            }
            if (extension == ".PMG")
            {
                format = ImageFormat.Png;
            }
            if (extension == ".TIF" || extension == ".TIFF")
            {
                format = ImageFormat.Tiff;
            }

            return format;
        }

        private bool IsImageFile(string extension)
        {
            string[] validImageFormats = {".tiff", ".tif", ".jpg", ".jpeg", ".bmp", ".png"};

            if (!extension.StartsWith(value:"."))
            {
                extension = $".{extension}";
            }

            return validImageFormats.Contains(value:extension.ToLower());
        }

        private byte[] ImageToByte(Image img, ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream:stream, format:format);
                return stream.ToArray();
            }
        }
    }
}