using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Authorization;
using System.Collections.Generic;

namespace Ncc.Roles.Dto
{
   
    public class PermissionDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        //public string Description { get; set; }

        public List<PermissionDto> Children { get; set; }
    }
}
