using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class ItemWatch
    {
        public string upc;
        public double price;
        public string name;

        public ItemWatch(string upc, double price, string name)
        {
            this.upc = upc;
            this.price = price;
            this.name = name;
        }
    }
}
