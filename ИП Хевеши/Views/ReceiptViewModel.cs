using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ИП_Хевеши.Views
{
    public class ReceiptViewModel
    {
        public int ID { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime Date { get; set; }
        public string ProviderName { get; set; }
        public string UserName { get; set; }

        // Для отображения в UI, если нужно
        public string DisplayText => $"{ReceiptNumber} от {Date:dd.MM.yyyy} ({ProviderName})";
    }
}
