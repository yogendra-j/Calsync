using CalendarSync.SystemCalendar.Dto;
using CalendarSync.SystemCalendar.Models;
using CalendarSyncPOC.Models;
using Microsoft.EntityFrameworkCore;
using SIRVA.Moving.Sales.GoogleCalendar;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSyncPOC
{
    public class DbContextAzure : DbContext
    {
        public DbContextAzure(DbContextOptions<DbContextAzure> options)
            : base(options)
        { }

        public DbSet<CalendarConfigurationSettings> CalendarConfigurationSettings { get; set; }
        public DbSet<User> AbpUsers { get; set; }
        public DbSet<OutlookTokenInfo> OutlookTokenInfos { get; set; }
        public DbSet<GoogleTokenInfo> GoogleTokenInfos { get; set; }
    }
}
