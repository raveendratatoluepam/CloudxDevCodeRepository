using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;

namespace AzuureCosmosDBfunction
{
    public static class Function1
    {
        // you get this values in CosmosDB/DataExplorer/keys
        private static readonly string _endpointUri = "https://cloudxdevcosmosdb.documents.azure.com:443/";
        private static readonly string _primaryKey = "T75KbAeEnL1XLU8ZUcjIb4q8z5Dn9D2JxYT8quCVkD5uaZ60q28E0h5WUpRbPpqWH8Ct7VduTTGC5duncFo0Ng==";
        [FunctionName("AzurecosmosDbFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                                
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Order data = JsonConvert.DeserializeObject<Order>(requestBody);

                using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
                {
                    DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync("OrdersForCloudX");
                    Database targetDatabase = databaseResponse.Database;
                    await Console.Out.WriteLineAsync($"Database Id:\t{targetDatabase.Id}");
                    IndexingPolicy indexingPolicy = new IndexingPolicy
                    {
                        IndexingMode = IndexingMode.Consistent,
                        Automatic = true,
                        IncludedPaths =
                    {
                        new IncludedPath
                        {
                            Path = "/*"
                        }
                    }
                    };
                    var containerProperties = new ContainerProperties("Items", "/OrderId")
                    {
                        IndexingPolicy = indexingPolicy
                    };
                    var containerResponse = await targetDatabase.CreateContainerIfNotExistsAsync(containerProperties, 10000);
                    var customContainer = containerResponse.Container;

                    await customContainer.CreateItemAsync(data, new PartitionKey(data.OrderId));
                    await Console.Out.WriteLineAsync($"Custom Container Id:\t{customContainer.Id}");
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }

            return new OkObjectResult("Job completed");
        }

        public class Order
        {
            public string id { get; set; }
            [JsonProperty("OrderId")]
            public string OrderId { get; set; }
            public Address ShippingAddress { get; set; }
            public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
            public int FinalPrice { get { return 100; } set { } }

        }
        public class Address // ValueObject
        {
            public string Street { get; private set; }

            public string City { get; private set; }

            public string State { get; private set; }

            public string Country { get; private set; }

            public string ZipCode { get; private set; }

            private Address() { }

            public Address(string street, string city, string state, string country, string zipcode)
            {
                Street = street;
                City = city;
                State = state;
                Country = country;
                ZipCode = zipcode;
            }
        }
        public class OrderItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Discount => 0;
            public int Units { get; set; }
            public string PictureUrl { get; set; }
        }
    }
}
