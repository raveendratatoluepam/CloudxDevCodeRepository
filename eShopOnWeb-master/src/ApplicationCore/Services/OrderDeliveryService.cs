using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class OrderDeliveryService
    {
        public async Task AddOrderDelivery(dynamic data)
        {
            var Url = "https://cloudxdevazuurecosmosdbfunction.azurewebsites.net/api/AzurecosmosDbFunction";

            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            using (var myclient = new HttpClient())
            using (var myrequest = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var httpContent = CreateHttpContent(data))
            {
                myrequest.Content = httpContent;

                using (var newresponse = await myclient
                    .SendAsync(myrequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken.Token)
                    .ConfigureAwait(false))
                {
                    var x = newresponse;
                }
            }
        }

        private static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var memorystresm = new MemoryStream();
                SerializeJsonIntoStream(content, memorystresm);
                memorystresm.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(memorystresm);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }


            return httpContent;
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var stremw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jsonw = new JsonTextWriter(stremw) { Formatting = Newtonsoft.Json.Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jsonw, value);
                jsonw.Flush();

            }
        }
    }
}