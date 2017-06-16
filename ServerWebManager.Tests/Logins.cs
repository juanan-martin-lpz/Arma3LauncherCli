using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;

namespace ServerWebManager.Tests
{

    public class LoginViewModel
    {
        public string grant_type { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    [TestClass]
    public class Logins
    {
        private string url = "http://localhost:60000/";
        private string auth_token = "";

        public string AuthToken => auth_token;

        private HttpResponseMessage getResponse(string URL, string urlParameters, string authorization = "")
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            if (authorization != "")
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
            }

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!

            return response;

        }

        private HttpResponseMessage LoginRequest(string URL, string user, string password)
        {

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            //client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "Authenticate");

            var ListParams = new List<KeyValuePair<string, string>>();

            ListParams.Add(new KeyValuePair<string, string>("grant_type", "password"));
            ListParams.Add(new KeyValuePair<string, string>("UserName", user));
            ListParams.Add(new KeyValuePair<string, string>("Password", password));

            request.Content = new FormUrlEncodedContent(ListParams);

            //HttpResponseMessage response = client.PostAsJsonAsync<LoginViewModel>(URL,login).Result;  // Blocking call!

            HttpResponseMessage response = client.SendAsync(request).Result;

            return response;

        }

        [TestMethod]
        public void TestConnect()
        {
            HttpResponseMessage response = getResponse(url, "", "");

            Assert.AreEqual(response.IsSuccessStatusCode, true);
        }

        public class Bearer
        {
            public string Access_Token { get; set; }
        }

        [TestMethod]
        public void GetAuthToken()
        {
            HttpResponseMessage response = LoginRequest(url,"Admin","secret");

            Assert.IsTrue(response.IsSuccessStatusCode);

            string result = response.Content.ReadAsStringAsync().Result;

            Newtonsoft.Json.Linq.JObject bearer = (Newtonsoft.Json.Linq.JObject) Newtonsoft.Json.JsonConvert.DeserializeObject(result);

            auth_token = bearer.Value<string>("access_token");

            Console.WriteLine("access token {0}", auth_token);

            Assert.AreNotEqual(auth_token, "");

        }

        [TestMethod]
        public void Dashboard()
        {

        }
    }
}
