using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPOC;
using CalendarSyncPOC.Models;
using Calsync.Models;
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
        private readonly AuthInfo authInfo;
        public Function1(DbContextAzure dbContext, AuthInfo authInfo)
        {
            this.dbContext = dbContext;
            this.authInfo = authInfo;
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
                var userList = new List<long>();
                userList.AddRange(await GetAllGoogleCalendarUsers());
                userList.AddRange(await GetAllOutlookCalendarUsers());
                userList = userList.Distinct().ToList();
                var authToken = await Login();
                log.LogCritical($"auth acc at: {DateTime.Now}");

                var endSearchDate = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                var startSearchDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                var url = $"https://localhost:44301/api/services/app/CalendarSync/SyncAllTeamDeltaEvents?startSearchDate={startSearchDate}&endSearchDate={endSearchDate}";
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", $"Bearer {authToken}");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(userList), ParameterType.RequestBody);
                log.LogCritical($"sync req sent at: {DateTime.Now}");
                IRestResponse response = await client.ExecuteAsync(request);
                log.LogCritical($"C# Timer trigger function completed at: {DateTime.Now}");
            }
            catch (Exception e)
            {
                log.LogCritical(e.Message);
                log.LogCritical(e.StackTrace);
            }
        }

        public async Task<List<long>> GetAllGoogleCalendarUsers()
        {
            try
            {
                return await dbContext.GoogleTokenInfos.Where(x => x.UserId != null).Select(x => (long)x.UserId).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<long>> GetAllOutlookCalendarUsers()
        {
            try
            {
                return await dbContext.OutlookTokenInfos.Where(x => x.UserId != null).Select(x => (long)x.UserId).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<string> Login()
        {
            try
            {
                var authmodel = new AuthenticateModel();
                authmodel.UserNameOrEmailAddress = authInfo.userEmail;
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
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
