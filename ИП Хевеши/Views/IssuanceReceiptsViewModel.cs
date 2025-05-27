using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ИП_Хевеши.Data;

namespace ИП_Хевеши.Views
{
    public class IssuanceReceiptsViewModel
    {
        public int ID { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime Date { get; set; }
        public string BuyerName { get; set; }
        public string UserName { get;set; }
    }
}
