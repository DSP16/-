using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ИП_Хевеши.Views
{
    internal class ProviderComponentsData
    {
        public string ProviderName { get; set; }
        public string ComponentName { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime ArrivalDate { get; set; }
    }
}
