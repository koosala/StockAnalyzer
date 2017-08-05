using System;
using System.Net.Http;
using System.Threading;
using System.IO;
using System.Text;
using StockAnalyzer.Cli.Web;

namespace StockAnalyzer.Cli.Web
{
    public class HttpRequestManager
    {
        
        public const string MethodPost = "POST";
        public const string MethodGet = "GET";
        public string Method { get; set }
        public string Uri { get; set; }
        public string PostContent { get; set; }
        private HttpClient Client { get; set; }
        public HttpRequestManager()
        {
            // Client = new HttpClient(GetHandler());
            Client = new HttpClient();
        }

        public string GetData()
        {
            var stringContent = new StringContent(PostContent, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage responseMessasge;
            if (Method == MethodPost)
                responseMessasge = Client.PostAsync(Uri, stringContent).Result;
            else 
                responseMessasge = Client.GetAsync(Uri).Result;
            var response = responseMessasge.Content.ReadAsStreamAsync().Result;

            var reader = new StreamReader(response);
            return reader.ReadToEnd();
        }

        private HttpClientHandler GetHandler()
        {
            // Gets a Fiddler Proxy
            WebProxy proxy = new WebProxy();
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Proxy = proxy,
                PreAuthenticate = true,
                UseDefaultCredentials = true,
            };

            return httpClientHandler;
        }
    }
}