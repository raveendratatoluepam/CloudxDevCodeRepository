using System;
using System.Collections.Generic;
using System.Text;

namespace OrderReservationFunction
{
    public  class StorageConfig
    {
        public string ConnectionString { get; set; } = "connectionString:DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=storageaccountcloudx;AccountKey=b7zHofUGces/KNeL1N4gGrpd3rxrFQ+akx0L8hz1Hllp8g7rYWXUffGJL+mBG2WuLGaM+SyT+ub63TUxFZYlqA==";
        public string OrderContainerName { get; set; } = "Order";
    }
}
