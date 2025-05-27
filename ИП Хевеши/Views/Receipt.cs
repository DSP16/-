using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ИП_Хевеши.Data;
namespace ИП_Хевеши.Views
{
    public class Receipt
    {
        public int ID { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime Date { get; set; }

        public int ProviderID { get; set; }
        public virtual Providers Provider { get; set; }

        public int UserID { get; set; }
        public virtual Users User { get; set; }

        public virtual ICollection<Arrivals> Arrivals { get; set; }
    }
}
