using System;
using System.Linq;
using CalendarSyncPOC;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CalendarSync
{
    public class Function1
    {
        private readonly DbContextAzure dbContext;
        public Function1(DbContextAzure dbContext)
        {
            this.dbContext = dbContext;
        }
        [FunctionName("Function1")]
        public void Run([TimerTrigger("0 */5 * * * *"
             #if DEBUG
            ,RunOnStartup=true
            #endif
            )]TimerInfo myTimer, ILogger log)
        {
            log.LogCritical($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                //Get the CSV contents
                var config = dbContext.CalendarConfigurationSettings.FirstOrDefault();

                log.LogCritical($"GoogleApiKey: {config?.GoogleApiKey}");
            }
            catch (Exception e)
            {
                log.LogCritical(e.Message);
                log.LogCritical(e.StackTrace);
            }
        }
    }
}
