using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderReservationFunction
{
    public interface IOrderReserver
    {
        Task Upload(dynamic data);
    }
}
