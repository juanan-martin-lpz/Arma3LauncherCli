using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Net.Security;

namespace ServerWebManager.Tests
{
    /// <summary>
    /// Descripción resumida de Servers
    /// </summary>
    [TestClass]
    public class Servers
    {
        LoginViewModel login;
        private string url = "http://localhost:60000";
        private string auth_token = "";

        public Servers()
        {
            //
            // TODO: Agregar aquí la lógica del constructor
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Obtiene o establece el contexto de las pruebas que proporciona
        ///información y funcionalidad para la serie de pruebas actual.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Atributos de prueba adicionales
        //
        // Puede usar los siguientes atributos adicionales conforme escribe las pruebas:
        //
        // Use ClassInitialize para ejecutar el código antes de ejecutar la primera prueba en la clase
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup para ejecutar el código una vez ejecutadas todas las pruebas en una clase
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Usar TestInitialize para ejecutar el código antes de ejecutar cada prueba 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup para ejecutar el código una vez ejecutadas todas las pruebas
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        

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

        private HttpResponseMessage postServer(string URL, ServerWebManager.Models.ServerSet server, string auth)
        {

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            if (auth != "")
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", auth);
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/Servers");

            var ListParams = new List<KeyValuePair<string, string>>();

            ListParams.Add(new KeyValuePair<string, string>("Name", server.Name));
            ListParams.Add(new KeyValuePair<string, string>("Description", server.Description));
            ListParams.Add(new KeyValuePair<string, string>("Port", server.Port));
            ListParams.Add(new KeyValuePair<string, string>("Private", server.Private.ToString()));
            ListParams.Add(new KeyValuePair<string, string>("RelativePath", server.RelativePath));
            ListParams.Add(new KeyValuePair<string, string>("OwnerId", server.OwnerId.ToString()));

            request.Content = new FormUrlEncodedContent(ListParams);

            //HttpResponseMessage response = client.PostAsJsonAsync<LoginViewModel>(URL,login).Result;  // Blocking call!

            HttpResponseMessage response = client.SendAsync(request).Result; 

            return response;

        }

        [TestMethod]
        public void GetAllServers()
        {
            HttpResponseMessage response = getResponse(url + "/api/Servers", "", "");

            Assert.AreEqual(response.IsSuccessStatusCode, true);
        }

        [TestMethod]
        public void CreateServer()
        {
            ServerWebManager.Models.ServerSet server = new Models.ServerSet() { Name = "Minimo", Description = "Minimo", Port = "2112", Private = false, RelativePath = @"\Minimo" };

            Logins login = new Logins();

            login.GetAuthToken();

            var response = postServer($"{url}/api/Servers", server, login.AuthToken);
            
            Assert.AreEqual(response.IsSuccessStatusCode, true);
        }
    }
}
