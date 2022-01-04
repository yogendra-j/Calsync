using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPOC;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        public async Task Run([TimerTrigger("0 */5 * * * *"
             #if DEBUG
            ,RunOnStartup=true
            #endif
            )]TimerInfo myTimer, ILogger log)
        {
            log.LogCritical($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                //Get the CSV contents
                var userList = new List<long>();
                userList.AddRange(await GetAllGoogleCalendarUsers());
                userList.AddRange(await GetAllOutlookCalendarUsers());
                userList = userList.Distinct().ToList();

                log.LogCritical($"GoogleApiKey: { string.Join(",", userList)}");

                var json = JsonConvert.SerializeObject(userList);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var url = "https://localhost:44301/api/services/app/CalendarSync/SyncAllTeamDeltaEvents";
                using var client = new HttpClient();

                var response = await client.PostAsync(url, data);
            }
            catch (Exception e)
            {
                log.LogCritical(e.Message);
                log.LogCritical(e.StackTrace);
            }
        }

        public async Task<List<long>> GetAllGoogleCalendarUsers()
        {
            return await dbContext.GoogleTokenInfos.Where(x => x.UserId != null).Select(x => (long)x.UserId).ToListAsync();
        }

        public async Task<List<long>> GetAllOutlookCalendarUsers()
        {
            return await dbContext.OutlookTokenInfos.Where(x => x.UserId != null).Select(x => (long)x.UserId).ToListAsync();
        }
    }
}
