using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OrderReservationFunction
{
    public class OrderReserverBlobService : IOrderReserver
    {
        protected readonly StorageConfig _storageConfig;
        protected readonly ILogger _log;
        protected CloudStorageAccount _storageAccount;
        protected CloudBlobClient _blobClient;
        protected CloudBlobContainer _container;
        protected ExecutionContext _executionContext;
        private const string _containerName = "order";
        public OrderReserverBlobService(StorageConfig config, ILogger log, ExecutionContext context)
        {
            _storageConfig = config;
            _log = log;
            _executionContext = context;
            Initialize();
        }
        private void Initialize()
        {
            _storageAccount = GetCloudStorageAccount(_executionContext);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _container = _blobClient.GetContainerReference(_containerName);
        }
        public async Task Upload(dynamic data)
        {
            _log.LogInformation($"C# Http trigger function executed at: {DateTime.Now}");
            CreateContainerIfNotExists(_log, _executionContext);
            UploadConentToBlob(data).GetAwaiter();
        }

        private async Task UploadConentToBlob(dynamic data)
        {
            string randomStr = Guid.NewGuid().ToString();
            CloudBlockBlob blob = _container.GetBlockBlobReference(randomStr);

            var serializeJesonObject = JsonConvert.SerializeObject(new { ID = randomStr, Content = data });
            blob.Properties.ContentType = "application/json";

            using (var ms = new MemoryStream())
            {
                ms.LoadStreamWithJson(serializeJesonObject);
                await blob.UploadFromStreamAsync(ms);
            }
            _log.LogInformation($"Bolb {randomStr} is uploaded to container {_container.Name}");
            blob.SetPropertiesAsync();
        }

        private void CreateContainerIfNotExists(ILogger logger, ExecutionContext executionContext)
        {
            _container.CreateIfNotExistsAsync();
        }
        private static CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=storageaccountcloudx;AccountKey=oRKTUCUxWCOvsTwU22wMo2IkgEumvkutCoy4wdfZg3NS92B1mPx7mAltdRpWj/kv9G6Obdy/FV1EG1YEC/0V1Q==");
            return storageAccount;
        }       
    }
}
