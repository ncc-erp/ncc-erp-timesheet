using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Ncc.EntityFrameworkCore
{
    public static class TimesheetDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<TimesheetDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<TimesheetDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
