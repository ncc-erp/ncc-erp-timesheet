using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ncc.MultiTenancy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Constants;
using Timesheet.Uitls;

namespace Timesheet.UploadFilesService
{
    public class UploadTeamBuildingService:ITransientDependency
    {
        private readonly IUploadFileService _uploadFileService;
        private readonly ILogger<UploadTeamBuildingService> _logger;
        private readonly TenantManager _tenantManager;
        private readonly IAbpSession _session;
        public UploadTeamBuildingService(IUploadFileService service,ILogger<UploadTeamBuildingService> logger, TenantManager tenantManager, IAbpSession session)
        {
            _uploadFileService = service;
            _logger = logger;
            _tenantManager = tenantManager;
            _session = session;
        }
        public Task<string> UploadTeamBuildingAsync(IFormFile file)
        {
            var tenantName = GetTenantName();
            return _uploadFileService.UploadTeamBuildingAsync(file, tenantName);
        }

        public async Task<List<string>> UploadMultipleTeamBuildingAsync(List<IFormFile> files)
        {
            if (files.IsNullOrEmpty()) return null;
            List<string> keys = new List<string>();
            foreach (var file in files)
            {
                var key = await UploadTeamBuildingAsync(file);
                keys.Add(key);
            }
            return keys;
        }
        private string GetTenantName()
        {
            if (_session.TenantId.HasValue)
            {
                var tenant = _tenantManager.GetById(_session.TenantId.Value);
                return tenant.TenancyName;
            }
            return "host";
        }

        public async Task<Dictionary<String, String>> UploadMultipleInvoiceFileTeamBuildingAsync(List<IFormFile> files)
        {
            if (files.IsNullOrEmpty()) return null;
            Dictionary<String, String> mapOldNameWithNewNameDictionary = new Dictionary<string, string>();
            foreach (var file in files)
            {
                var key = await UploadTeamBuildingAsync(file);
                mapOldNameWithNewNameDictionary.Add(file.FileName, key);
            }
            return mapOldNameWithNewNameDictionary;
        }
    }
}
