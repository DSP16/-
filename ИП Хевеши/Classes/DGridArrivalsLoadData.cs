using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ИП_Хевеши.UI.Winds;
using ИП_Хевеши.Data;
using ИП_Хевеши.Classes;
using ИП_Хевеши.UI.Pages;
using ИП_Хевеши.UI.UserControls;
using System.Collections.ObjectModel;

namespace ИП_Хевеши.Classes
{
    public class DGridArrivalsLoadData 
    {
        
        
        public static List<Arrivals> GetArrivalsList()
        {
            using(var context = new ИП_ХевешиEntities())
            {
                return context.Arrivals.ToList();
            }
        }
       
      
    }
}
