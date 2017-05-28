using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;

namespace APITesting
{
    public class ServerConfigViewModel
    {
        public string PublicServersPath { get; set; }
        public string RepositoryPath { get; set; }
        public string SteamCmdPath { get; set; }
        public string PrivateServersPath { get; set; }
    }

    // Modelos devueltos por las acciones de RepositoryController

    public class RepositoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, ModFileViewModel> Mods { get; set; }
    }

    public class ModViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ModFileViewModel> Files { get; set; }
    }

    public class ModFileViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }            // Nombre del archivo
        public string RelativePath { get; set; }    // Path relativo al path del mod
        public int Length { get; set; }
        public string Signature { get; set; }
    }

    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main()
        {
            RunAsync().Wait();
        }

        static void ShowConfig(ServerConfigViewModel config)
        {
            Console.WriteLine($"Server Path: {config.PublicServersPath}\tRepository Path: {config.RepositoryPath}\tSteamCMD Path: {config.SteamCmdPath}");
        }


        static async Task<ServerConfigViewModel> GetServerConfig(string path)
        {
            ServerConfigViewModel server = null;
            HttpResponseMessage response = await client.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                server = await response.Content.ReadAsAsync<ServerConfigViewModel>();
            }
            return server;
        }

        static async Task<ServerConfigViewModel> SetServerConfig(string path, ServerConfigViewModel config)
        {
            ServerConfigViewModel server = config;
            //HttpContent content = new StringContent(JsonConvert.SerializeObject(config));

            HttpResponseMessage response = await client.PostAsJsonAsync(path, server);


            if (response.IsSuccessStatusCode)
            {
                server = await response.Content.ReadAsAsync<ServerConfigViewModel>();
            }
            return server;
        }

        /*
        static async Task<Stream> GetFile()
        {

            Stream file = File.Open(@"E:\downloaded.zip", FileMode.CreateNew, FileAccess.ReadWrite);

            //Stream file = new FileStream(@"E:\downloaded.zip", FileMode.CreateNew, FileAccess.ReadWrite);

            HttpResponseMessage response = await client.GetAsync("/api/ServerConfig/GetFile");

            if (response.IsSuccessStatusCode)
            {
                file = await response.Content.ReadAsStreamAsync();
            }

            Console.WriteLine("Archivo recibido : {0} bytes", file.Length);

            return file;
        }
        */

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("http://localhost:60531/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ServerConfigViewModel config = new ServerConfigViewModel();
            
            config = await GetServerConfig("/api/ServerConfig/GetConfig");
            ShowConfig(config);

            config.PublicServersPath = "E:\\Servidores\\PrivateServer\\";
            config = await SetServerConfig("/api/ServerConfig/SetConfig", config);
            ShowConfig(config);

            config = await GetServerConfig("/api/ServerConfig/GetConfig");
            ShowConfig(config);

            /*
            var file = await GetFile();

            file.Flush();
            file.Close();

            Console.WriteLine("Archivo guardado en el disco");
            */

            Console.ReadLine();
        }

    }
}
