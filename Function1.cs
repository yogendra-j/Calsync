using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPOC;
using CalendarSyncPOC.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

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
        public async Task Run([TimerTrigger("0 */1 * * * *"
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
                var authToken = await Login();
                log.LogCritical($"GoogleApiKey: { string.Join(",", userList)}");

                var json = JsonConvert.SerializeObject(userList);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var url = "https://localhost:44301/api/services/app/CalendarSync/SyncAllTeamDeltaEvents";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
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

        public async Task<string> Login()
        {
            var authmodel = new AuthenticateModel();
            authmodel.UserNameOrEmailAddress = @"yogendra.jaiswal.009@gmail.com";
            authmodel.Password = @"123qwe";
            var json = JsonConvert.SerializeObject(authmodel);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = "https://localhost:44301/api/TokenAuth/Authenticate";
            using var client1 = new HttpClient();

            var response = await client1.PostAsync(url, data);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                JObject jobject = JObject.Parse(result);
                return JsonConvert.DeserializeObject<AuthenticateResultModel>(jobject["result"].ToString()).AccessToken;
            }
            return "";
        }
    }
}
