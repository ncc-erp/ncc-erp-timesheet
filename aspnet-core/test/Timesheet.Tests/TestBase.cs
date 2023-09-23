using Abp.Domain.Uow;
using Abp.Modules;
using Abp.TestBase;
using Timesheet.Tests.Seeders;
using Microsoft.Extensions.DependencyInjection;
using Ncc.Authorization;
using Ncc.EntityFrameworkCore;
using Ncc.EntityFrameworkCore.Seed.Host;
using Ncc.EntityFrameworkCore.Seed.Tenants;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Tests
{
    public abstract class TesBase<TStartupModule> : AbpIntegratedTestBase<TStartupModule> where TStartupModule : AbpModule
    {
        protected TesBase()
        {
            //SeedUserData();
            SeedData();
            LoginAsHostAdmin();
        }

        public void UsingDbContext(Action<TimesheetDbContext> action)
        {
            using (var context = Resolve<TimesheetDbContext>())
            {
                action(context);
                context.SaveChanges();
            }
        }

        protected virtual async Task<TResult> WithUnitOfWorkAsync<TResult>(Func<Task<TResult>> func)
        {
            using (var scope = Resolve<IServiceProvider>().CreateScope())
            {
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                using (var uow = uowManager.Begin())
                {
                    var result = await func();
                    await uow.CompleteAsync();
                    return result;
                }
            }
        }

        protected virtual async Task WithUnitOfWorkAsync(Func<Task> func)
        {
            using (var scope = Resolve<IServiceProvider>().CreateScope())
            {
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                using (var uow = uowManager.Begin())
                {
                    await func();
                    await uow.CompleteAsync();
                }
            }
        }

        private void SeedData()
        {
            UsingDbContext((context) =>
            {
                new DataSeederConsumer().Seed(context);
            });
        }

        private void SeedUserData()
        {
            UsingDbContext((context) =>
            {
                new InitialHostDbBuilder(context).Create();
                new DefaultTenantBuilder(context).Create();
            });
        }

        private void LoginAsHostAdmin()
        {
            var logInManager = Resolve<LogInManager>();
            var loginResult = logInManager.LoginAsync("admin", "123qwe").Result;
            AbpSession.UserId = loginResult.User.Id;
            AbpSession.TenantId = loginResult.User.TenantId;
        }
    }
}
