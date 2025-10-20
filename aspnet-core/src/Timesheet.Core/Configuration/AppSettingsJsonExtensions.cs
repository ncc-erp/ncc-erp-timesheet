using Microsoft.Extensions.Configuration;
using System.IO;

namespace Timesheet.Configuration
{
    public static class AppSettingsJsonExtensions
    {
        public static void AddAppSettingsJson(this IConfigurationBuilder builder, string environmentName = null)
        {
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
            }
        }
    }
}
