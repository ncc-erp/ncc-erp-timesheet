using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Ncc.Configuration;
using Ncc.Web;

namespace Ncc.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class TimesheetDbContextFactory : IDesignTimeDbContextFactory<TimesheetDbContext>
    {
        public TimesheetDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TimesheetDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            TimesheetDbContextConfigurer.Configure(builder, configuration.GetConnectionString(TimesheetConsts.ConnectionStringName));

            return new TimesheetDbContext(builder.Options);
        }
    }
}
