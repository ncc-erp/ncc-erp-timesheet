using Abp;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Uitls;

namespace Ncc.IoC
{
    public class WorkScope : AbpServiceBase, IWorkScope
    {
        private readonly IIocManager _iocManager;
        public WorkScope(IIocManager iocManager)
        {
            _iocManager = iocManager;
        }
        IQueryable<TEntity> IWorkScope.GetAll<TEntity, TPrimaryKey>()
        {
            return (this as IWorkScope).GetRepo<TEntity, TPrimaryKey>().GetAll();
        }

        IQueryable<TEntity> IWorkScope.GetAll<TEntity>()
        {
            return (this as IWorkScope).GetRepo<TEntity, long>().GetAll();
        }

        IQueryable<TEntity> IWorkScope.All<TEntity>()
        {
            return (this as IWorkScope).GetRepo<TEntity, long>().GetAll();
        }

        IRepository<TEntity, TPrimaryKey> IWorkScope.GetRepo<TEntity, TPrimaryKey>()
        {
            var repoType = typeof(IRepository<,>);
            Type[] typeArgs = { typeof(TEntity), typeof(TPrimaryKey) };
            var repoGenericType = repoType.MakeGenericType(typeArgs);
            var resolveMethod = _iocManager.GetType()
                .GetMethods()
                .First(s => s.Name == "Resolve" && !s.IsGenericMethod && s.GetParameters().Length == 1 && s.GetParameters()[0].ParameterType == typeof(Type));
            var repo = resolveMethod.Invoke(_iocManager, new object[] { repoGenericType });
            return repo as IRepository<TEntity, TPrimaryKey>;
        }

        IRepository<TEntity, long> IWorkScope.GetRepo<TEntity>()
        {
            return (this as IWorkScope).GetRepo<TEntity, long>();
        }

        IRepository<TEntity, long> IWorkScope.Repository<TEntity>()
        {
            return (this as IWorkScope).GetRepo<TEntity, long>();
        }

        TEntity IWorkScope.Clone<TEntity>(TEntity entity)
        {
            entity.Id = 0;
            return (this as IWorkScope).GetRepo<TEntity, long>().Insert(entity);
        }

        long IWorkScope.CloneAndGetId<TEntity>(TEntity entity)
        {
            entity.Id = 0;
            return (this as IWorkScope).GetRepo<TEntity, long>().InsertAndGetId(entity);
        }

        IEnumerable<TEntity> IWorkScope.InsertRange<TEntity>(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                yield return (this as IWorkScope).GetRepo<TEntity, long>().Insert(entity);
            }
        }

        async Task<IEnumerable<TEntity>> IWorkScope.InsertRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        {
            var updatedEntities = new List<TEntity>();
            foreach (var entity in entities)
            {
                updatedEntities.Add(await (this as IWorkScope).GetRepo<TEntity, long>().InsertAsync(entity));
            }

            return updatedEntities;
        }

        public TEntity Insert<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return (this as IWorkScope).GetRepo<TEntity, long>().Insert(entity);
        }

        public async Task<TEntity> InsertAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return await (this as IWorkScope).GetRepo<TEntity, long>().InsertAsync(entity);
        }

        public long InsertAndGetId<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return (this as IWorkScope).GetRepo<TEntity, long>().InsertAndGetId(entity);
        }

        public async Task<long> InsertAndGetIdAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return await (this as IWorkScope).GetRepo<TEntity, long>().InsertAndGetIdAsync(entity);
        }


        public TEntity Update<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return (this as IWorkScope).GetRepo<TEntity, long>().Update(entity);
        }

        public async Task<TEntity> UpdateAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return await (this as IWorkScope).GetRepo<TEntity, long>().UpdateAsync(entity);
        }

        public long InsertOrUpdateAndGetId<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return (this as IWorkScope).GetRepo<TEntity, long>().InsertOrUpdateAndGetId(entity);
        }

        public async Task<long> InsertOrUpdateAndGetIdAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            return await (this as IWorkScope).GetRepo<TEntity, long>().InsertOrUpdateAndGetIdAsync(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            (this as IWorkScope).GetRepo<TEntity, long>().Delete(entity);
        }

        public void Delete<TEntity>(long id) where TEntity : class, IEntity<long>
        {
            (this as IWorkScope).GetRepo<TEntity, long>().Delete(id);
        }

        public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<long>
        {
            await (this as IWorkScope).GetRepo<TEntity, long>().DeleteAsync(entity);
        }

        public async Task DeleteAsync<TEntity>(long id) where TEntity : class, IEntity<long>
        {
            await (this as IWorkScope).GetRepo<TEntity, long>().DeleteAsync(id);
        }

        public void SoftDelete<TEntity>(TEntity entity) where TEntity : class, IEntity<long>, ISoftDelete
        {
            entity.IsDeleted = true;
            (this as IWorkScope).GetRepo<TEntity, long>().Update(entity);
        }

        public async Task SoftDeleteAsync<TEntity>(TEntity entity) where TEntity : class, IEntity<long>, ISoftDelete
        {
            entity.IsDeleted = true;
            await (this as IWorkScope).GetRepo<TEntity, long>().UpdateAsync(entity);
        }

        public TEntity Get<TEntity>(long id) where TEntity : class, IEntity<long>
        {
            return (this as IWorkScope).GetRepo<TEntity, long>().Get(id);
        }

        public async Task<TEntity> GetAsync<TEntity>(long id) where TEntity : class, IEntity<long>
        {
            return await (this as IWorkScope).GetRepo<TEntity, long>().GetAsync(id);
        }

        IEnumerable<TEntity> IWorkScope.UpdateRange<TEntity>(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                yield return (this as IWorkScope).GetRepo<TEntity, long>().Update(entity);
            }
        }

        async Task<IEnumerable<TEntity>> IWorkScope.UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        {
            var updatedEntities = new List<TEntity>();
            foreach (var entity in entities)
            {
                updatedEntities.Add(await (this as IWorkScope).GetRepo<TEntity, long>().UpdateAsync(entity));
            }

            return updatedEntities;
        }

        public async Task<IEnumerable<TEntityDto>> Sync<TEntityDto, TEntity>(IEnumerable<TEntityDto> input)
            where TEntity : class, IEntity<long>
            where TEntityDto : class, IEntityDto<long?>
        {
            return await Sync<TEntityDto, TEntity>(input, x => false, x => x);
        }
        public async Task<IEnumerable<TEntityDto>> Sync<TEntityDto, TEntity>(IEnumerable<TEntityDto> input, Expression<Func<TEntity, bool>> condition)
            where TEntity : class, IEntity<long>
            where TEntityDto : class, IEntityDto<long?>
        {
            return await Sync<TEntityDto, TEntity>(input, condition, x => x);
        }

        public async Task<IEnumerable<TEntityDto>> Sync<TEntityDto, TEntity>(
            IEnumerable<TEntityDto> input,
            Expression<Func<TEntity, bool>> condition,
            Func<TEntityDto, TEntityDto> merge
        )
            where TEntity : class, IEntity<long>
            where TEntityDto : class, IEntityDto<long?>
        {
            var repo = (this as IWorkScope).GetRepo<TEntity, long>();
            if (input == null)
            {
                await repo.DeleteAsync(condition);
                return null;
            }

            var currentItems = await repo.GetAll().Where(condition).ToListAsync();
            var newItems = input.ToList();
            foreach (var item in currentItems)
            {
                var newItem = newItems.FirstOrDefault(t => item.Id == t.Id);
                if (newItem != null)
                {
                    newItem = merge(newItem);
                    Mapper.Map(newItem, item);
                    repo.Update(item);
                }
                else
                {
                    repo.Delete(item);
                }
            }
            foreach (var item in newItems.Where(s => !s.Id.HasValue))
            {
                var newItem = item;
                newItem = merge(newItem);
                item.Id = await repo.InsertAndGetIdAsync(Mapper.Map<TEntity>(newItem));
            }

            return newItems;
        }
        public async Task DeleteRangeAsync<TEntity>(List<TEntity> entities) where TEntity : class, IEntity<long>
        {
            var entityType = typeof(TEntity);
            var isDeleted = entityType.GetProperty("IsDeleted");
            //var deleterUserId = entityType.GetProperty("DeleterUserId");
            var deletionTime = entityType.GetProperty("DeletionTime");
            foreach (var entity in entities)
            {
                isDeleted.SetValue(entity, true);
                deletionTime.SetValue(entity, DateTimeUtils.GetNow());
                //deleterUserId.SetValue(entity, AbpSession.UserId);
            }
        }


    }
}
