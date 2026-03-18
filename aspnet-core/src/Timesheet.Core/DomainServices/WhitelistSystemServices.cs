using Abp.Linq.Extensions;
using Abp.Dependency;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Ncc.IoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Timesheet.DomainServices.Dto;
using Timesheet.Entities;
using System.Linq;
using static Ncc.Entities.Enum.StatusEnum;
using Timesheet.Uitls;

namespace Timesheet.DomainServices
{
    public class WhitelistSystemServices : BaseDomainService, IWhitelistSystemServices, ITransientDependency
    {
        private readonly IWorkScope _workScope;
        public WhitelistSystemServices(IWorkScope workScope) : base(workScope)
        {
            _workScope = workScope;
        }

        public async Task<WhitelistSystem> Add(AddWhitelistTypeDto input)
        {
            try
            {
                if (!input.Type.HasValue)
                {
                    throw new UserFriendlyException("Whitelist type is required.");
                }

                var isDuplicatedType = await _workScope.GetAll<WhitelistSystem>()
                    .Where(x => x.Type == input.Type.Value)
                    .AnyAsync();
                if (isDuplicatedType)
                {
                    throw new UserFriendlyException($"Whitelist type '{input.Type.Value}' already exists.");
                }

                var whitelistSystem = new WhitelistSystem
                {
                    Name = input.Name,
                    Code = input.Code,
                    Description = input.Description,
                    Type = input.Type.Value,
                    IsActive = input.IsActive
                };

                whitelistSystem.Id = await _workScope.InsertAndGetIdAsync(whitelistSystem);

                return whitelistSystem;
            }
            catch (Exception ex)
            {
                if (ex is UserFriendlyException)
                {
                    throw;
                }
                throw new UserFriendlyException("An error occurred while adding a new whitelist type", ex);
            }
        }

        public async Task<WhitelistSystem> Update(UpdateWhitelistTypeDto input)
        {
            try
            {
                var whitelistSystem = await _workScope.GetAsync<WhitelistSystem>(input.Id);

                if (!string.IsNullOrEmpty(input.Name))
                {
                    whitelistSystem.Name = input.Name;
                }

                if (!string.IsNullOrEmpty(input.Code))
                {
                    whitelistSystem.Code = input.Code;
                }

                if (input.Type.HasValue)
                {
                    whitelistSystem.Type = input.Type.Value;
                }

                whitelistSystem.Description = input.Description;

                if (input.IsActive != whitelistSystem.IsActive)
                {
                    whitelistSystem.IsActive = input.IsActive;
                }    
                    
                await _workScope.UpdateAsync(whitelistSystem);
                return whitelistSystem;
            }
            catch (Exception ex)
            {
                if (ex is UserFriendlyException)
                {
                    throw;
                }
                throw new UserFriendlyException("An error occurred while updating the whitelist type", ex);
            }
        }

        public List<WhitelistTypeDto> GetWhitelistTypes()
        {
            return CommonUtils.WhitelistTypeName()
                .Select(x => new WhitelistTypeDto
                {
                    Value = (int)x.Key,
                    Name = x.Value
                })
                .ToList();
        }

        public async Task<List<GetWhitelistSystemDto>> GetAll()
        {
            try
            {
                var result = await _workScope.GetAll<WhitelistSystem>()
                    .Select(x => new GetWhitelistSystemDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Code = x.Code,
                        Description = x.Description,
                        Type = x.Type,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while retrieving whitelist types", ex);
            }
        }

        public async Task<bool> Delete(long id)
        {
            try
            {
                await _workScope.DeleteAsync<WhitelistSystem>(id);
                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while deleting the whitelist type", ex);
            }
        }
    }
}
