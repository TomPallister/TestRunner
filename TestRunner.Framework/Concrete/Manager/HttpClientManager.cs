using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestRunner.Framework.Concrete.Enum;
using TestRunner.Framework.Concrete.Model;

namespace TestRunner.Framework.Concrete.Manager
{
    public class HttpClientManager : IDisposable
    {
        private readonly HttpClient _httpClient;

        public HttpClientManager(HttpClient client)
        {
            _httpClient = client;
        }
        public string Get(string address)
        {
            return _httpClient.GetStringAsync(address).Result;
        }
        public void Post(TestRun testRun, string address)
        {
            var json = JsonConvert.SerializeObject(testRun);
            var httpContent = new StringContent(json);
            var response =  _httpClient.PostAsync(address, httpContent).Result;
            response.EnsureSuccessStatusCode();
        }
        public void Put(TestRun testRun, string address)
        {
            var json = JsonConvert.SerializeObject(testRun);
            var httpContent = new StringContent(json);
            var response = _httpClient.PutAsync(address, httpContent).Result;
            response.EnsureSuccessStatusCode();
        }
        public void Post(TestRecord testRecord, string address)
        {
            var json = JsonConvert.SerializeObject(testRecord);
            var httpContent = new StringContent(json);
            var response = _httpClient.PostAsync(address, httpContent).Result;
            //added this logging temp to work out a bug.
            Log4NetLogger.LogEntry(GetType(), "HttpClientManager.Post(TestRecord testRecord, string address)", response.Content.ToString(), LoggerLevel.Info);
            response.EnsureSuccessStatusCode();
        }
        public async Task Post(List<TestCoverage> testCoverages, string address)
        {
            var json = JsonConvert.SerializeObject(testCoverages);
            var httpContent = new StringContent(json);
            var response = await _httpClient.PostAsync(address, httpContent);
            response.EnsureSuccessStatusCode();
        }
        public void Dispose()
        {
            _httpClient.Dispose();   
        }
    }
}
