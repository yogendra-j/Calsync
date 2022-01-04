using CalendarSync.SystemCalendar;
using CalendarSyncPOC.Models;
using Microsoft.EntityFrameworkCore;
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
        public DbSet<ServiceCalendarEvent> ServiceCalendarEvent { get; set; }
    }
}
