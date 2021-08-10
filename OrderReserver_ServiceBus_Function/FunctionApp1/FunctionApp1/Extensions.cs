using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OrderReservationFunction
{
    public static class Extensions
    {
        public static void LoadStreamWithJson(this Stream ms, object obj)
        {
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(obj);
            writer.Flush();
            ms.Position = 0;
        }
    }
}
