using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderReserverFunction
{
    public static class OrderReserverFunction
    {
        [FunctionName("OrderReserverFunction")]
        public static void Run([ServiceBusTrigger("order", Connection = "ServiceBusConnectionString")]Message myQueueItem, ILogger log)
        {
            var body = Encoding.UTF8.GetString(myQueueItem.Body);
            log.LogInformation(body);

            try
            {
                TriggerFunction(body);
            }
            catch
            {

                TriggerLogicApp(body);
            }
        }

        private static void TriggerLogicApp(object data)
        {
            var Url = "https://prod-10.northcentralus.logic.azure.com:443/workflows/c420859e72054de9b576e1de2247e4f9/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=s3nBaqIS5MtORe3JrSzRBJF5C8XrkXq2hdotE_Lm_i4";

            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            using (var myclient = new HttpClient())
            using (var myrequest = new HttpRequestMessage(HttpMethod.Post, Url))
            using (var httpContent = CreateHttpContent(data))
            {
                myrequest.Content = httpContent;

                using (var newresponse = myclient
                    .SendAsync(myrequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken.Token)
                    .ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    if (newresponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception("Got failed result");
                    }
                }
            }
        }
        private static void TriggerFunction(object data)
        {
            var Url = "https://cloudxdevorderreservationfunction.azurewebsites.net/api/OrderReservationHttpTrigger";

            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            using (var myclient = new HttpClient())
            using (var myrequest = new HttpRequestMessage(HttpMethod.Get, Url))
            using (var httpContent = CreateHttpContent(data))
            {
                myrequest.Content = httpContent;

                using (var newresponse =  myclient
                    .SendAsync(myrequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken.Token)
                    .ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    if (newresponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception("Unable to add order !!");
                    }
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
            using (var jsonw = new JsonTextWriter(stremw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jsonw, value);
                jsonw.Flush();
            }
        }

    }
}
