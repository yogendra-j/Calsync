using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Calsync.Models;

[assembly: FunctionsStartup(typeof(CalendarSyncPOC.Startup))]

namespace CalendarSyncPOC
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings:Default");
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            var authInfo = new AuthInfo();
            config.Bind(nameof(AuthInfo), authInfo);

            builder.Services.AddSingleton(authInfo);
            builder.Services.AddDbContext<DbContextAzure>(
                options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, connectionString));
        }
    }
}
